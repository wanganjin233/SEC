using Microsoft.AspNetCore.Hosting;
using Microsoft.VisualBasic;
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
          var ad=  FileTransition.FileBytes("C:\\Users\\su\\source\\repos\\sec\\SEC.DockerSupporter\\Driver\\publish.zip");
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
                socketServer.DisconnectEvent += SocketServer_DisconnectEvent;
            }, stoppingToken);
        } 

        private void SocketServer_DisconnectEvent(Socket socket)
        {
            using (Sockets.Write())
            {
                Sockets.Data?.Remove(socket);
            }
        }
        protected UsingLock<Dictionary<Socket,string>> Sockets = new(new Dictionary<Socket, string>());

        private void SocketServer_ReceiveEvent(Socket socket, byte[] bytes)
        {
            var operateResult = bytes.ToString(Encoding.UTF8).ToObject<OperateResult<object>>();
            if (operateResult?.IsSuccess ?? false)
            {
                switch (operateResult.Message)
                {
                    case "register"://驱动注册
                        string? equ = operateResult.Content?.ToString();
                        if (!string.IsNullOrEmpty(equ))
                        {
                            using (Sockets.Write())
                            {
                                FileInfo fi = new FileInfo(Path);
                                byte[] buff = new byte[fi.Length];
                                FileStream fs = fi.OpenRead();
                                fs.Read(buff, 0, Convert.ToInt32(fs.Length));
                                fs.Close();
                                return buff;


                                Sockets.Data?.Add(socket, equ);
                            } 
                        } 
                        break;
                    default:
                        break;
                }
            }

        }
    }
}

