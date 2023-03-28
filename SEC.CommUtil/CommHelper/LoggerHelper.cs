using Microsoft.Extensions.Logging; 
using SEC.Models.Interactive; 

namespace SEC.CommUtil
{
    public class LoggerHelper : ILoggerProvider
    {
        private readonly Action<LogInfo> _SendAction;
        public LoggerHelper(Action<LogInfo> SendAction)
        {
            _SendAction = SendAction;
        }

        public ILogger CreateLogger(string categoryName)
        {
            return new MyLogger(categoryName, _SendAction);
        }

        public void Dispose()
        {
        }
        class MyLogger : ILogger
        {
            private readonly string _CategoryName;
            public MyLogger(string categoryName, Action<LogInfo> SendAction)
            {
                _SendAction = SendAction;
                _CategoryName = categoryName;
            }
            private readonly Action<LogInfo> _SendAction;
           
            public bool IsEnabled(LogLevel logLevel)
            {
                return true;
            }

            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
            {
                _SendAction?.Invoke(new LogInfo()
                {
                    CategoryName = _CategoryName,
                    LogLevel = (int)logLevel,
                    Message = formatter.Invoke(state, exception),
                    State = state
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
