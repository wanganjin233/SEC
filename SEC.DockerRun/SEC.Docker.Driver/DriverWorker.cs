using SEC.Driver;
using SEC.Models.Driver;
using SEC.Models.Interactive;
using SEC.Util;
using System.Reflection;
using System.Runtime.InteropServices;

namespace SEC.Docker.Driver
{
    public class DriverWorker : BackgroundService
    {
        /// <summary>
        /// socket客户端
        /// </summary>
        private readonly SockerHelper socketClient;
        /// <summary>
        /// 配置
        /// </summary>
        private EquConfig? equInfo = null;
        ILogger<DriverWorker> _logger;
        string equ = "H-ME-MD-023";
        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="logger"></param>
        public DriverWorker(ILogger<DriverWorker> logger, SockerHelper sockerHelper)
        {
            _logger = logger;
            socketClient = sockerHelper;
            string basePath = Environment.GetEnvironmentVariable("basePath", EnvironmentVariableTarget.Process) ?? AppDomain.CurrentDomain.BaseDirectory;
            string infoPath = Path.Combine(basePath, "info");

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                equ = Environment.GetEnvironmentVariable("equ", EnvironmentVariableTarget.Process) ?? equ;
            }
            if (!socketClient.Connected())
            {
                _logger.LogError("连接core失败,尝试使用本地配置");
                if (File.Exists(infoPath))
                {
                    _logger.LogInformation("使用本地配置文件");
                    string equInfoJson = File.ReadAllText(infoPath);
                    equInfo = equInfoJson.ToObject<EquConfig>();
                }
                else
                {
                    _logger.LogError("未找到本地配置文件5秒后关闭");
                    Thread.Sleep(5000);
                    Environment.Exit(0);
                }
            }
            else
            {
                _logger.LogInformation($"获取设备【{equ}】配置");
                equInfo = GetConfig(equ);
            }
            if (equInfo != null)
            {
                _logger.LogInformation($"获取设备【{equ}】配置成功");
                File.WriteAllText(infoPath, equInfo.ToJson());
            }
            else
            {
                _logger.LogError($"下载设备【{equ}】配置失败5秒后关闭");
                Task.Delay(5000).Wait();
                Environment.Exit(0);
            }

        }

        /// <summary>
        /// 执行内容
        /// </summary>
        /// <param name="stoppingToken"></param>
        /// <returns></returns>
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await Task.Run(() =>
            {
                try
                {
                    Assembly assembly = Assembly.LoadFrom(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $"SEC.Driver.OPCUA.dll"));
                    var type = assembly.GetType($"SEC.Driver.OPCUA");
                    if (type != null)
                    {
                        if (Activator.CreateInstance(type, equInfo.ConnectionString) is BaseDriver baseDriver)
                        {
                            AppDomain.CurrentDomain.ProcessExit += (object? sender, EventArgs e) =>
                            {
                                _logger.LogError($"释放{equInfo.ConnectionString} 设备【{equInfo.Name}】 设备编号【{equInfo.EQU}】 驱动类型【{equInfo.DriverType}】");
                                baseDriver.Dispose();
                            };
                            if (baseDriver.DriverState)
                            {
                                _logger.LogInformation($"连接{equInfo.ConnectionString}成功");
                                baseDriver.AddTags(equInfo.Tags);
                                baseDriver.DriverStateChange += (BaseDriver obj) =>
                                {
                                    if (!obj.DriverState)
                                    {
                                        _logger.LogError($"断开连接 设备【{equInfo.Name}】 设备编号【{equInfo.EQU}】 驱动类型【{equInfo.DriverType}】");
                                    }
                                };
                                baseDriver.AllTags.ForEach(p =>
                                {
                                    p.ValueChangeEvent += ValueChangeEvent;
                                });
                                socketClient.WriteTagEvent += (Tag tag) =>
                                {
                                    socketClient.Send(new OperateResult<string>()
                                    {
                                        IsSuccess = baseDriver.Write(tag, tag.Value),
                                        Message = "",
                                    });
                                };
                                baseDriver.Start();
                            }
                            else
                            {
                                baseDriver.Stop();
                                _logger.LogError($"无法连接{equInfo.ConnectionString} 设备【{equInfo.Name}】 设备编号【{equInfo.EQU}】 驱动类型【{equInfo.DriverType}】");
                                Thread.Sleep(3000);
                                Environment.Exit(0);
                            }
                        }
                        else
                        {
                            _logger.LogError($"驱动初始化异常，设备【{equInfo.Name}】 设备编号【{equInfo.EQU}】 驱动类型【{equInfo.DriverType}】");
                            Thread.Sleep(3000);
                            Environment.Exit(0);
                        }
                    }
                }
                catch (Exception e)
                {
                    _logger.LogError($"驱动初始化异常，设备【{equInfo.Name}】 设备编号【{equInfo.EQU}】 驱动类型【{equInfo.DriverType}】 错误信息 【{e.Message}】");
                    Thread.Sleep(3000);
                    Environment.Exit(0);
                }

            }, stoppingToken);
        }

        /// <summary>
        /// 点位变化事件
        /// </summary>
        /// <param name="tag"></param>
        private void ValueChangeEvent(Tag tag)
        {
            socketClient.Send(new OperateResult<Tag>()
            {
                SenderIdentity = equ,
                Content = tag,
                Message = "ResponseMethod/ReportedTag",
                IsSuccess = true
            });
        }
        /// <summary>
        /// 获取配置文件
        /// </summary>
        /// <returns></returns>
        private EquConfig? GetConfig(string equ)
        {
            EquConfig? operateResultEquInfo = socketClient.SendWait<EquConfig, string>(new OperateResult<string>()
            {
                IsSuccess = true,
                Message = "ResponseMethod/GetConfig",
                Content = equ
            });

            return operateResultEquInfo;
        }
    }
}