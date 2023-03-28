using SEC.Models.Driver;
using SEC.Models.Interactive;
using SEC.Util;
using System.Collections.Concurrent;

namespace SEC.CommUtil
{
    internal class SubTagManage
    {
        public static ConcurrentDictionary<string, Tag> SubTagEvents = new ConcurrentDictionary<string, Tag>();
        /// <summary>
        /// 订阅 
        /// </summary>
        /// <param name="sockerClient"></param>
        /// <param name="TagNames"></param>
        /// <returns></returns>
        public static List<Tag> Subs(SocketClientHelper sockerClient, List<string> TagNames, string Router)
        {
            TagNames = TagNames.Where(p => !SubTagEvents.ContainsKey(p)).ToList();
            List<Tag> tags = new List<Tag>();
            var subsData = new OperateResult<object>()
            {
                Router = Router,
                Content = TagNames
            };
            sockerClient.ConnectSendDatas.Add(subsData);
            OperateResult<List<Tag>>? operateResult = sockerClient.SendWait<object, List<Tag>>(subsData);
            if(operateResult?.Content != null)
            {
                tags = operateResult.Content;
                foreach (var tag in tags)
                {
                    SubTagEvents.TryAdd(tag.TagName, tag);
                }
            } 
            return tags;
        }
    }
}
