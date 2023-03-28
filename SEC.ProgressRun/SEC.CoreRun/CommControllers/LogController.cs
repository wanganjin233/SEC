using SEC.Interface.Interactive;
using SEC.Models.Interactive;

namespace SEC.CoreRun.CommControllers
{
    public class LogController : ICommController
    {
        private readonly ILoggerFactory _loggerFactory;
        public LogController(ILoggerFactory loggerFactory)
        {
            _loggerFactory = loggerFactory;
        }
        /// <summary>
        /// 日志记录
        /// </summary>
        /// <param name="tag"></param>
        /// <returns></returns>
        public void Log(LogInfo logInfo)
        {
            _loggerFactory.CreateLogger(logInfo.CategoryName).Log((LogLevel)logInfo.LogLevel, logInfo.Message);
        }
    }
}
