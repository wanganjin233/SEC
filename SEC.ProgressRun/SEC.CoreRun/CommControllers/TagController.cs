using SEC.CommUtil;
using SEC.CoreRun.Manage;
using SEC.Interface.Interactive;
using SEC.Models.Driver;
using SEC.Models.Interactive;

namespace SEC.CoreRun.CommControllers
{
    public class TagController : ICommController
    {
        private readonly ILogger<TagController> _logger;
        private readonly SocketServerHelper _sockerServer;
        public TagController(ILoggerFactory loggerFactory, SocketServerHelper sockerServer)
        {
            _logger = loggerFactory.CreateLogger<TagController>();
            _sockerServer = sockerServer;

        }
        /// <summary>
        ///Tag点位变化
        /// </summary>
        /// <returns></returns>
        public void TagChange(SocketClientInfo socketClientInfo, Tag tag)
        {
            _logger.LogInformation($"收到点位 {tag.Address}【{tag.Value}】");
            //更新服务上的点位
            if (EquInfoManage.UpdateTag(socketClientInfo.EQU, tag))
            {
                //获取点位订阅端信息并推送
                var subInfos = EquInfoManage.GetTag(socketClientInfo.EQU, tag.TagName)?.GetSubInfo();
                if (subInfos != null)
                {
                    foreach (var subInfo in subInfos)
                    {
                        BaseCommands.TagPush(subInfo, _sockerServer, tag);
                    }
                }
            }
        }
        /// <summary>
        /// 订阅点位
        /// </summary>
        public List<Tag> SubTags(SocketClientInfo socketClientInfo, List<string> subTags)
        {
            _logger.LogInformation($"收到点位订阅【{string.Join(",", subTags)}】");
            return PushTagManage.SubTag(socketClientInfo, subTags);
        }
    }
}
