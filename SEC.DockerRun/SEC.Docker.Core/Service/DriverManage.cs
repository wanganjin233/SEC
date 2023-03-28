using Docker.DotNet.Models;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using SEC.Enum.Driver;
using SEC.Models.Interactive;
using SEC.Communication;
using SEC.Interface.Interactive;
using SEC.Util;
using System.Data;
using SEC.Enum.Docker;
using Dapper.Contrib.Extensions;
using System.Diagnostics;

namespace SEC.Docker.Core
{
    public class DriverManage : BackgroundService
    {
        /// <summary>
        /// docker管理
        /// </summary>
        DockerManage dockerManage;
        /// <summary>
        /// 驱动配置
        /// </summary>
        public Dictionary<string, string> ConfigFiles = new();
        /// <summary>
        /// 日志工厂
        /// </summary>
        private readonly ILoggerFactory _loggerFactory;
        /// <summary>
        /// 日志记录
        /// </summary>
        private readonly ILogger<DriverManage> _logger;
        /// <summary>
        /// Socket服务
        /// </summary>
        private readonly SocketServer socketServer;
        /// <summary>
        /// 方法
        /// </summary>
        Dictionary<string, MethodInfo> Routes = new Dictionary<string, MethodInfo>();

        private readonly SqliteHelper sqliteHelper;

        private IConfiguration _configuration;

        private KafkaHelper kafkaHelper;
        /// <summary>
        /// 初始化驱动管理
        /// </summary>
        /// <param name="loggerFactory"></param>
        /// <param name="configuration"></param>
        public DriverManage(
            ILoggerFactory loggerFactory,
            IConfiguration configuration,
            SqliteHelper sqliteHelper,
            DockerManage dockerManage,
            KafkaHelper kafkaHelper)
        {
            this.kafkaHelper = kafkaHelper;
            this.sqliteHelper = sqliteHelper;
            this.dockerManage = dockerManage;
            _logger = loggerFactory.CreateLogger<DriverManage>();
            _loggerFactory = loggerFactory;
            _configuration = configuration;
            #region 获取接口下所有方法
            foreach (var type in GetType().Assembly.GetTypes().Where(p => p.GetInterface(typeof(ICommController).Name) != null))
            {
                foreach (var methodInfo in type.GetMethods().Where(p => p.DeclaringType == type))
                {
                    Routes.Add($"{type.Name}.{methodInfo.Name}".Replace(".", "/").ToLower(), methodInfo);
                }
            }

            #endregion
            //监听30000端口
            socketServer = new SocketServer(30000, 253)
            {
                HeadBytes = new byte[2] { 0, 1 },
                EndBytes = new byte[2] { 2, 3 },
                DataLengthLocation = 2,
                DataLengthType = LengthTypeEnum.Uint,
            };
            //接收回调
           //socketServer.ReceiveEvent += SocketServer_ReceiveEvent;
           ////断开回调
           //socketServer.DisconnectEvent += (Socket socket) =>
           //{
           //    _logger.LogInformation($"会话{socket.RemoteEndPoint}已断开");
           //};
           //socketServer.ConnectEvent += (Socket socket) =>
           //{
           //    _logger.LogInformation($"会话{socket.RemoteEndPoint}已连接");
           //};
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            { 
                List<EquState> equStates = new List<EquState>();
                using (IDbConnection db = sqliteHelper.Connection)
                {
                    equStates.AddRange(db.GetAll<EquState>().Where(p => p.Enable));
                }

                //  ProcessStartInfo startInfo = new ProcessStartInfo();
                //  startInfo.FileName = "D:\\SEC\\SEC.DockerRun\\SEC.Docker.Driver\\bin\\Debug\\net7.0\\SEC.Docker.Driver.exe";
                //  startInfo.Arguments = "";
                //  startInfo.WorkingDirectory = "D:\\SEC\\SEC.DockerRun\\SEC.Docker.Driver\\bin\\Debug\\net7.0";
                //  startInfo.WindowStyle = ProcessWindowStyle.Minimized;
                //  Process p = Process.Start(startInfo);
                //  var asd = p.Id;


                //创建网络
                // IList<NetworkResponse> networkResponses = await dockerManage.GetNetworksAsync();
                // if (!networkResponses.Any(p => p.Name == "DriverCore"))
                // {
                //     await dockerManage.CreateNetworkAsync("DriverCore", NetworkDriver.bridge);
                // }
                await EnableDriverAsync(equStates);
            }
            catch (Exception)
            {

                
            }
        }
        /// <summary>
        /// 启动容器
        /// </summary>
        /// <param name="equStates"></param>
        /// <returns></returns>
        public async Task EnableDriverAsync(List<EquState> equStates)
        {
            string? fromImage = _configuration["FromImage"];
            string? serverAddress = _configuration["ServerAddress"];
            if (fromImage == null || serverAddress == null)
            {
                return;
            }
            await dockerManage.PullImageAsync(fromImage);
            //获取全部容器
            IList<ContainerListResponse> containers = await dockerManage.GetContainersAsync();
            foreach (var equState in equStates)
            {
                //创建对应卷
                if (!dockerManage.GetVolumesAsync().Result.Any(p => p.Name == equState.Equ))
                {
                    await dockerManage.CreateVolumesAsync(equState.Equ);
                }
                //获取对应容器
                ContainerListResponse? container = containers.FirstOrDefault(p => p.Names.First() == $"/{equState.Equ}");
                if (container == null)
                {
                    CreateContainerResponse createContainerResponse = await dockerManage.CreateContainerAsync(
                                equState.Equ,
                                fromImage,
                                envs: new List<string>()
                                    {
                                         $"equ={equState.Equ}",
                                         $"address={serverAddress}",
                                         $"basePath=/config"
                                    },
                                binds: new List<string>()
                                {
                                    $"{equState.Equ}:/config"
                                },
                                endpoints: new List<string>()
                                {
                                    "host"
                                });
                    await dockerManage.StartContainerAsync(createContainerResponse.ID);
                }
                else if (container.State != "running")
                {
                    await dockerManage.StartContainerAsync(container.ID);
                }
            }
        }


        private void SocketServer_ReceiveEvent(Socket socket, byte[] bytes)
        {
            if (bytes.Length == 0) return;
            var operateResult = bytes.ToString(Encoding.UTF8).ToObject<OperateResult<object>>();
            if (operateResult != null)
            {
                Routes.TryGetValue(operateResult.Message.ToLower(), out MethodInfo? methodInfo);
                if (methodInfo != null)
                {
                    List<object> parametors = new List<object>();
                    ParameterInfo[] paramsInfos = methodInfo.GetParameters();
                    foreach (var paramsInfo in paramsInfos)
                    {
                        if (operateResult.Content != null)
                        {
                            var parameter = operateResult.Content.ToJson()?.ToObject(paramsInfo.ParameterType);
                            if (parameter != null)
                            {
                                parametors.Add(parameter);
                            }
                        }
                        else break;
                    }
                    if (paramsInfos.Length <= parametors.Count && methodInfo.DeclaringType != null)
                    {
                        try
                        {
                            object? instance = Activator.CreateInstance(methodInfo.DeclaringType,
                                new object[] {
                                    _loggerFactory,
                                    sqliteHelper,
                                    kafkaHelper,
                                    operateResult.SenderIdentity
                                });

                            object? result = methodInfo.Invoke(instance, parametors.ToArray());
                            if (result != null)
                            {
                                Type resultType = result.GetType();
                                if (typeof(OperateResult<>) == resultType.GetGenericTypeDefinition())
                                {
                                    PropertyInfo? property = resultType.GetProperty("MessageCode");
                                    property?.SetValue(result, operateResult.MessageCode);
                                }
                                string resultJson = result.ToJson();
                               // socketServer.SendConformity(socket, resultJson.ToBytes(Encoding.UTF8));
                            }
                        }
                        catch (Exception e)
                        {
                            _logger.LogError($"回调异常{e.Message}");
                            throw;
                        }
                    }
                }
            }
        }
    }
}

