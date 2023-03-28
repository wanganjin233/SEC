using SEC.CommUtil;
using SEC.Models.Interactive;

namespace SEC.DriversRun
{
    public class Program
    {
        public static void Main(string[] args)
        {
            SocketClientHelper sockerClient = new(args[0],
                new SocketClientInfo
                {
                    ClientType = "SEC.DriversRun",
                    EQU = args[1],
                    ProgressId = Environment.ProcessId
                });
            IHost host = Host.CreateDefaultBuilder(args)
                .ConfigureServices(services =>
                {
                    services.AddHostedService<DriverWorker>();
                    services.AddSingleton(sockerClient);
                    services.AddLogging(logging =>
                    {
                        logging.SetMinimumLevel(LogLevel.Trace);
                        logging.AddProvider(new LoggerHelper((logInfo) =>
                        {
                            CoreCommands.Log(sockerClient, logInfo);
                        }));
                    });
                })
                .Build();

            host.Run();
        }
    }
}