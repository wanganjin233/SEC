using SEC.Communication;
using SEC.CommUtil;
using SEC.Driver;
using SEC.Models.Driver;
using SEC.Models.Interactive;
using SEC.Util;

namespace SEC.DriversRun
{
    public class DriverWorker : BackgroundService
    {
        /// <summary>
        /// socker客户端连接
        /// </summary>
        private readonly SocketClientHelper _SockerClientHelper;
        /// <summary>
        /// 配置
        /// </summary>
        private readonly EquConfig? equInfo = null;
        private readonly ControllerManage controllerManage;
        /// <summary>
        /// 日志服务
        /// </summary>
        private readonly ILogger<DriverWorker> _logger;
        /// <summary>
        /// 日志服务
        /// </summary>
        private readonly ILoggerFactory _loggerFactory;
        private readonly BaseDriver baseDriver;
        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="logger"></param>
        public DriverWorker(ILogger<DriverWorker> logger, ILoggerFactory loggerFactory, SocketClientHelper SockerClient)
        {
            _loggerFactory = loggerFactory;
            _logger = logger;
            _SockerClientHelper = SockerClient;
            string config = File.ReadAllText(Path.Combine(Config.EnableConfigPath, $"{SockerClient.ScokerCilentInfo.EQU}.json"));
            equInfo = config.ToObject<EquConfig>();
            if (equInfo == null)
            {
                _logger.LogError("未找到配置");
                Thread.Sleep(3000);
                Environment.Exit(0);
            }
            baseDriver = equInfo.DriverType switch
            {
                "Fins" => new Fins(equInfo.ConnectionString),
                "MC3E" => new MC3E(equInfo.ConnectionString),
                "ModbusRtu" => new ModbusRtu(equInfo.ConnectionString),
                "ModbusTcp" => new ModbusTcp(equInfo.ConnectionString),
                "OPCUA" => new OPCUA(equInfo.ConnectionString),
                _ => throw new NotImplementedException(),
            };
            AppDomain.CurrentDomain.ProcessExit += (object? sender, EventArgs e) =>
            {
                //_logger.LogError($"释放{equInfo.ConnectionString} 设备【{equInfo.Name}】 设备编号【{equInfo.EQU}】 驱动类型【{equInfo.DriverType}】");
                baseDriver.Dispose();
            };
            controllerManage = new ControllerManage(_loggerFactory, baseDriver);
            SockerClient.ReceiveEvent += SockerClient_ReceiveEvent;
        }

        /// <summary>
        /// 接收事件
        /// </summary>
        /// <param name="SockerClient"></param>
        /// <param name="data"></param>
        private void SockerClient_ReceiveEvent(SocketClientHelper SockerClient, OperateResult<object> operateResult)
        {
            try
            {
                object? obj = controllerManage.MethodInfoInvoke(operateResult, SockerClient);
                if (operateResult.SenderIdentity != null)
                {
                    operateResult.Content = obj;
                    _SockerClientHelper.Reply(operateResult);
                }
            }
            catch (Exception e)
            {
                _logger.LogError($"处理接收信息异常【{e.Message}】"); 
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
                if (equInfo != null)
                {
                    if (baseDriver.DriverState)
                    {
                        _logger.LogInformation($"连接{equInfo.ConnectionString}成功");
                        baseDriver.AddTags(equInfo.Tags);
                        baseDriver.DriverStateChange += (BaseDriver obj) =>
                        {
                            if (!obj.DriverState)
                            {
                                _logger.LogError($"断开连接 设备【{equInfo.Name}】 设备编号【{equInfo.EQU}】 驱动类型【{equInfo.DriverType}】");
                                Environment.Exit(0);
                            }
                        };
                        baseDriver.AllTags.ForEach(p =>
                        {
                            p.ValueChangeEvent += ValueChangeEvent;
                        });
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
            }, stoppingToken);
        }
        /// <summary>
        /// 点位变化事件
        /// </summary>
        /// <param name="tag"></param>
        private void ValueChangeEvent(Tag tag)
        {
#if DEBUG
            //    Console.WriteLine($"点位名称【{tag.TagName}】点位地址【{tag.Address}】点位值【{tag.Value}】");
#endif
            CoreCommands.TagEvent(_SockerClientHelper,tag); 
        }
    }
}