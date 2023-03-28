using SEC.Models.Interactive;
using SEC.Util;

namespace SEC.Docker.Driver
{
    public class LoggerHelper : ILoggerProvider
    {
        readonly SockerHelper sockerHelper;
        public LoggerHelper(SockerHelper sockerHelper)
        {
            this.sockerHelper = sockerHelper;
        }

        public ILogger CreateLogger(string categoryName)
        {
            return new MyLogger(sockerHelper);
        }

        public void Dispose()
        {
        }
        class MyLogger : ILogger
        {
            public MyLogger(SockerHelper sockerHelper)
            {
                this.sockerHelper = sockerHelper;
            }
            SockerHelper sockerHelper;

            public bool IsEnabled(LogLevel logLevel)
            {
                return true;
            }

            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
            {

                sockerHelper.Send(new OperateResult<object>
                {
                    IsSuccess = true,
                    Message = "ResponseMethod/ReportedLog",
                    Content = new
                    {
                        CategoryName = Environment.GetEnvironmentVariable("equ", EnvironmentVariableTarget.Process),
                        LogLevel = logLevel,
                        Message = $"时间:【{DateTime.UtcNow}】 {formatter.Invoke(state, exception)}"
                    }
                });
            }

            class Disposable : IDisposable
            {
                public void Dispose()
                {

                }
            }
            public IDisposable BeginScope<TState>(TState state) where TState : notnull
            {
                return new Disposable();
            }
        }
    }
}
