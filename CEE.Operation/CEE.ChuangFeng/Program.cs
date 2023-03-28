using SEC.CommUtil;
using SEC.Models.Interactive;

namespace CEE.ChuangFeng
{
    public class Program
    {
        public static void Main(string[] args)
        {
            SocketClientHelper sockerClient = new(args[0], new SocketClientInfo
            {
                ClientType = "CEE.DefaultRecipe",
                EQU = args[1],
                ProgressId = Environment.ProcessId
            });
            IHost host = Host.CreateDefaultBuilder(args)
                .ConfigureServices(services =>
                {
                    services.AddHostedService<Worker>();
                })
                .Build();

            host.Run();
        }
    }
}