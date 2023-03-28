using Newtonsoft.Json.Linq;
using SEC.Models.Driver;
using SEC.Models.Interactive; 
using SEC.Interface.Interactive;
using SEC.Util;
using Dapper.Contrib.Extensions;

namespace SEC.Docker.Core
{
    public class ResponseMethod : ICommController
    {
        public readonly ILoggerFactory _loggerFactory;
        public readonly SqliteHelper sqliteHelper;
        public readonly ILogger<ResponseMethod> _logger;
        public readonly KafkaHelper kafkaHelper;
        public readonly string Identifier;
        public ResponseMethod(ILoggerFactory loggerFactory, SqliteHelper sqliteHelper, KafkaHelper kafkaHelper, string Identifier)
        {
            this.sqliteHelper = sqliteHelper;
            _loggerFactory = loggerFactory;
            _logger = loggerFactory.CreateLogger<ResponseMethod>();
            this.kafkaHelper = kafkaHelper;
            this.Identifier = Identifier;
        }
        /// <summary>
        /// 获取配置
        /// </summary>
        /// <returns></returns>
        public OperateResult<Models.Driver.EquConfig> GetConfig(string equ)
        {
            try
            {
                using (var connection = sqliteHelper.Connection)
                {
                    EquConfig? driverVersions = connection.GetAll<EquConfig>()
                        .Where(p => p.Equ == equ)
                        .OrderByDescending(p => new Version(p.Version))
                        .FirstOrDefault();
                    if (driverVersions != null)
                    {
                        string config = File.ReadAllText(driverVersions.ConfigPath);
                        return new OperateResult<Models.Driver.EquConfig>()
                        {
                            IsSuccess = true,
                            Message = "Config",
                            Content = config.ToObject<Models.Driver.EquConfig>()
                        };
                    }
                    else
                    {
                        return new OperateResult<Models.Driver.EquConfig>()
                        {
                            IsSuccess = false,
                            Message = "Config"
                        };
                    }
                }
            }
            catch (Exception e)
            {
                _logger.LogError($"发送【{equ}】配置文件异常 【{e.Message}】");
                return new OperateResult<Models.Driver.EquConfig>()
                {
                    IsSuccess = false,
                    Message = "Config"
                };
            }
        } 
        /// <summary>
        /// 接收点位变化
        /// </summary>
        /// <param name="Tag"></param>
        /// <returns></returns>
        public void ReportedTag(Tag Tag)
        {

           // string  message = "\"{\\\"Name\\\":\\\""+ Tag.Value + 
           //     "\\\",\\\"Value\\\":\\\"" + Tag.Value + 
           //     "\\\",\\\"Timestamp\\\":\\\""+ Tag.Timestamp +
           //     "\\\",\\\"Quality\\\":192,\\\"Device\\\":\\\"HANDrilling14,M-ME-MD-024\\\",\\\"OldValue\\\":\\\"11371:02:05\\\",\\\"DataType\\\":\\\"String\\\",\\\"Filed\\\":\\\"0\\\"}\""
            string? message = new
            {
                Name = Tag.TagName,
                Tag.Value,
                Tag.Timestamp,
                Quality = 192,
                Device = $"55,{Identifier}",
                Tag.OldValue,
                DataType = Tag.DataType.ToString(),
                Filed="0"
            }.ToJson(); 
                kafkaHelper.ProduceMessage("system-tag-value", message);
       
        }
        /// <summary>
        /// 接收日志
        /// </summary>
        /// <param name="log"></param>
        public void ReportedLog(JObject log)
        {
            var logLevel = (LogLevel?)log["LogLevel"]?.Value<int>();
            var message = log["Message"]?.Value<string>();
            var categoryName = log["CategoryName"]?.Value<string>();
            if (logLevel != null && message != null && categoryName != null)
            {
                _loggerFactory.CreateLogger(categoryName).Log((LogLevel)logLevel, message);
            }
        }

    }
}
