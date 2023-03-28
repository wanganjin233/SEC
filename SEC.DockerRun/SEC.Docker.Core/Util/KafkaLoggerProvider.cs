using SEC.Util; 
namespace SEC.Docker.Core
{
    public class KafkaLoggerProvider : ILoggerProvider
    {
        public ILogger CreateLogger(string categoryName)
        {
            var services = new ServiceCollection();
            var provider = services.BuildServiceProvider();
            var kafkaHelper = provider.GetService<KafkaHelper>(); 
            return new KafkaLogger(categoryName, kafkaHelper);
        }

        public void Dispose()
        {

        }
        class KafkaLogger : ILogger
        {
            public KafkaLogger(string categoryName, KafkaHelper? kafkaHelper)
            {
                localIp = IpHelper.GetLocalIp();
                _categoryName = categoryName;
                _KafkaHelper = kafkaHelper;
            }
            KafkaHelper? _KafkaHelper;
            private readonly string localIp;
            public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

            public bool IsEnabled(LogLevel logLevel)
            {
                return true;
            }
            private readonly string _categoryName;
            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
            {
                _KafkaHelper?.ProduceMessage("system-service-log", new
                {
                    Topic = "",
                    Key = "Supporter",
                    Message = new
                    {
                        Category = "Supporter",
                        LogLevel = (int)logLevel,
                        State = state,
                        Message = formatter.Invoke(state, exception),
                        ApplicationName = _categoryName,
                        Time = DateTime.UtcNow,
                        IpAddress = localIp
                    }
                }.ToJson());
            }
        }
    }
}
