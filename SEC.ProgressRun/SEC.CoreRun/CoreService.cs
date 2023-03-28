using SEC.Models.Interactive;
using SEC.Util;
using System.Data;
using SEC.Models.Driver;
using System.Diagnostics;
using System.Runtime.InteropServices;
using SEC.CommUtil;
using SEC.CoreRun.Manage;

namespace SEC.CoreRun
{

    public class CoreService : BackgroundService
    {
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
        private readonly ILogger<CoreService> _logger;
        /// <summary>
        /// 队列
        /// </summary>
        private readonly TaskQueue taskQueue = new TaskQueue();

        private readonly SocketServerHelper sockerServerHelper;

        private readonly ControllerManage receiveManage;
        /// <summary>
        /// 初始化驱动管理
        /// </summary>
        /// <param name="loggerFactory"></param>
        /// <param name="configuration"></param>
        public CoreService(ILoggerFactory loggerFactory, RabbitMQHelper rabbitMQ)
        {
            _logger = loggerFactory.CreateLogger<CoreService>();
            _loggerFactory = loggerFactory;
            sockerServerHelper = new SocketServerHelper(10000, "CoreService");
            sockerServerHelper.ReceiveEvent += SockerServerHelper_ReceiveEvent;
            receiveManage = new ControllerManage(_loggerFactory, this, sockerServerHelper, rabbitMQ);
        }
        /// <summary>
        /// 执行任务
        /// </summary>
        /// <param name="stoppingToken"></param>
        /// <returns></returns>
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await Task.Run(() =>
            {
                #region 启动对应服务端  
                string[] enableConfgPaths = Directory.GetFiles(Config.EnableConfigPath);
                foreach (var enableConfgPath in enableConfgPaths)
                {
                    EquConfig? equInfo = File.ReadAllText(enableConfgPath).ToObject<EquConfig>();
                    if (equInfo != null)
                    {
                        //去除重复启动进程配置
                        equInfo.Operations = equInfo.Operations
                        .Where((x, i) => equInfo.Operations.FindIndex(s => s == x) == i)
                        .ToList();
                        EquInfoManage.Add(equInfo);
                    }
                }
                #endregion
                #region 检测进程状态 
                while (true)
                {
                    Task.Delay(5000).Wait();
                    foreach (var equConfig in EquInfoManage.EquConfigs)
                    {
                        List<string> ProgressRunings = new List<string>();
                        foreach (var scokerCilentInfo in sockerServerHelper.ScokerCilentInfos.Where(p => p.Value.EQU == equConfig.Value.EQU))
                        {
                            if (ProgressRunings.Contains(scokerCilentInfo.Value.ClientType)//进程重复
                            || !equConfig.Value.Operations.Contains(scokerCilentInfo.Value.ClientType))//进程不在需要运行的清单内
                            {
                                ThreadPool.QueueUserWorkItem(p =>
                                {
                                    _logger.LogInformation($"尝试关闭进程【{scokerCilentInfo.Value.ProgressId}】");
                                    if (BaseCommands.Kill(scokerCilentInfo.Key, sockerServerHelper))
                                    {
                                        _logger.LogInformation($"进程【{scokerCilentInfo.Value.ProgressId}】已关闭");
                                    }
                                    else
                                    {
                                        try
                                        {
                                            Process.GetProcessById(scokerCilentInfo.Value.ProgressId).Kill();
                                            _logger.LogInformation($"强制关闭进程【{scokerCilentInfo.Value.ProgressId}】");
                                        }
                                        catch (Exception)
                                        {
                                            sockerServerHelper.ScokerCilentInfos.Remove(scokerCilentInfo.Key, out _);
                                        }
                                    }
                                });
                            }
                            else
                            {
                                ProgressRunings.Add(scokerCilentInfo.Value.ClientType);
                            }
                        }
                        //遍历启动未在运行的进程
                        foreach (var _needRun in equConfig.Value.Operations.Where(p => !ProgressRunings.Contains(p)))
                        {
                            string runFile = string.Empty;
                            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                            {
                                runFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $"{_needRun}.exe");
                            }
                            else
                            {
                                runFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, _needRun);
                            }
                            try
                            {
                                _logger.LogInformation($"启动【{runFile}】设备号【{equConfig.Value.EQU}】");
                                Process p = Process.Start(runFile, new string[] { $"{Config.LocalIp}:10000", equConfig.Value.EQU });
                            }
                            catch (Exception)
                            {
                                _logger.LogError($"无法启动【{runFile}】");
                            }
                        }

                    }
                }
                #endregion
            }, stoppingToken);
        }

        /// <summary>
        /// 处理接收信息
        /// </summary>
        /// <param name="id"></param>
        /// <param name="socketClient"></param>
        /// <param name="operateResult"></param>
        private void SockerServerHelper_ReceiveEvent(string id, SocketClientInfo socketClient, OperateResult<object> operateResult)
        {
            taskQueue.Enqueue(() =>
            {
                //没有标注接收标识由服务端处理
                if (operateResult.ReceiverIdentity == null)
                {
                    object? obj = receiveManage.MethodInfoInvoke(operateResult, socketClient);
                    //根据发送标识返回信息
                    if (operateResult.SenderIdentity != null)
                    {
                        operateResult.Content = obj;
                        sockerServerHelper.Reply(id, operateResult);
                    }
                }
                else//发给目标服务处理
                {
                    string? pipelineId = sockerServerHelper.ScokerCilentInfos.FirstOrDefault(p => operateResult.ReceiverIdentity == p.Value.ClientId).Key;
                    if (pipelineId != null)
                    {
                        sockerServerHelper.Send(pipelineId, operateResult);
                    }
                    else
                    {
                        _logger.LogError($"未找到转发目标{operateResult.ReceiverIdentity}");
                    }
                }
            });


        }
    }
}

