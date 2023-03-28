using SEC.Driver;
using SEC.Interface.Interactive;
using SEC.Models.Driver;

namespace SEC.DriversRun
{
    public class DriverController : ICommController
    {
        private readonly ILogger<DriverController> _logger;
        private readonly BaseDriver _baseDriver;

        public DriverController(ILoggerFactory loggerFactory, BaseDriver baseDriver)
        {
            _logger = loggerFactory.CreateLogger<DriverController>();
            _baseDriver = baseDriver;
        }
        /// <summary>
        /// 写入tag
        /// </summary>
        /// <param name="tag"></param>
        /// <returns></returns>
        public bool WriteTag(Tag tag)
        {
            bool isOk = false;
            try
            {
                Tag? _tag = _baseDriver.AllTags.FirstOrDefault(p => p.TagName == tag.TagName);
                if (_tag != null)
                {
                    isOk = _baseDriver.Write(_tag, tag.Value);
                    if (isOk)
                    {
                        _logger.LogInformation($"写入:【{tag.TagName}】:【{_tag.Address}】值:【{tag.Value}】成功");
                    }
                    else
                    {
                        _logger.LogError($"写入:【{tag.TagName}】地址:【{_tag.Address}】读写:【{_tag.ClientAccess}】值:【{tag.Value}】失败");
                    }
                }
                else
                {
                    _logger.LogError($"未找到【{tag.TagName}】点位 ");
                }
            }
            catch (Exception e)
            {

                _logger.LogError($"写入【{tag.TagName}】点位 异常 【{e.Message}】");
            }
            return isOk;
        }

        /// <summary>
        /// 获取所有Tag点位
        /// </summary>
        /// <returns></returns>
        public List<Tag> GetAllTag()
        {
            return _baseDriver
                        .AllTags
                        .Select(p => p as Tag)
                        .ToList();
        } 
    }
}
