using SEC.DockerSupporter.Controllers;
using SEC.Driver;
using SEC.Util;
using System.Net.Sockets;
using System.Text;

namespace SEC.DockerSupporter
{
    public class DriverManage : BackgroundService
    {
        private readonly ILogger<DriverUpdaterController> _logger;
        public DriverManage(ILogger<DriverUpdaterController> logger)
        {
            _logger = logger;
        }
        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            return Task.Run(() =>
            {
                SocketServer socketServer = new SocketServer(30000, 100)
                {
                    HeadBytes = new byte[2] { 1, 2 },
                    EndBytes = new byte[2] { 3, 4 },
                    DataLengthLocation = 2,
                    DataLengthType = LengthTypeEnum.Uint
                };
                socketServer.ReceiveEvent += SocketServer_ReceiveEvent;

            }, stoppingToken);
        }

        private void SocketServer_ReceiveEvent(Socket socket, byte[] bytes)
        {
            var operateResult = bytes.ToString(Encoding.UTF8).ToObject<OperateResult<object>>();
            if (operateResult?.IsSuccess ?? false)
            {

            }

        }
    }
}

