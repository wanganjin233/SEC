using SEC.CommUtil;
using SEC.CoreRun.Manage;
using SEC.Interface.Interactive;
using SEC.Models.Driver;
using SEC.Models.Interactive;

namespace SEC.CoreRun.SockerControllers
{
    public class MQController : ICommController
    {
        private readonly ILogger<MQController> _logger;
        private readonly SocketServerHelper _sockerServer;
        private readonly RabbitMQHelper _rabbitMQ;
        public MQController(ILoggerFactory loggerFactory, SocketServerHelper sockerServer, RabbitMQHelper rabbitMQ)
        {
            _logger = loggerFactory.CreateLogger<MQController>();
            _sockerServer = sockerServer;
            _rabbitMQ = rabbitMQ;

        }
        /// <summary>
        /// 订阅点位
        /// </summary>
        public List<Tag> SubMQs(SocketClientInfo socketClientInfo, List<string> routes)
        {
            _logger.LogInformation($"收到MQ订阅【{string.Join(",", routes)}】");
            return PushMqManage.SubMqQueue(socketClientInfo, _sockerServer, _rabbitMQ, routes);
        }
        /// <summary>
        /// 发送点位信息
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        public bool PubMQ(string date)
        {
            _logger.LogInformation($"收到MQ信息【{date}】");
            return PushMqManage.PubMqQueue(_rabbitMQ, date);
        }
    }
}
