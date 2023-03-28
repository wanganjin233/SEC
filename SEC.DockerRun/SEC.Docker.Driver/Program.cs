namespace SEC.Docker.Driver
{
    public class Program
    {
        public static void Main(string[] args)
        {
            SockerHelper sockerHelper = new SockerHelper();
            IHost host = Host.CreateDefaultBuilder(args)
                .ConfigureServices(services =>
                {
                    services.AddHostedService<DriverWorker>();
                    services.AddSingleton(sockerHelper);
                    services.AddLogging(logging =>
                    {
                        logging.SetMinimumLevel(LogLevel.Trace);
                        logging.AddProvider(new LoggerHelper(sockerHelper));
                    });
                })
                .Build();

            host.Run();
        }
    }
}