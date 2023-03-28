using SEC.Models.Driver;
using SEC.Models.Interactive;
using System.Collections.Concurrent;

namespace SEC.CoreRun.Manage
{

    public static class PushTagManage
    {
        /// <summary>
        /// 订阅的点位
        /// </summary>
        private static readonly ConcurrentDictionary<Tag, List<string>> SubTags = new();

        /// <summary>
        /// 获取订阅端信息
        /// </summary>
        /// <param name="tag"></param>
        /// <returns></returns>
        public static List<string>? GetSubInfo(this Tag tag)
        {
            SubTags.TryGetValue(tag, out List<string>? subPipelineServerId);
            return subPipelineServerId;
        }

        /// <summary>
        /// 订阅点位
        /// </summary>
        /// <param name="socketClientInfo"></param>
        /// <param name="subTags"></param>
        /// <returns></returns>
        public static List<Tag> SubTag(SocketClientInfo socketClientInfo, List<string> subTags)
        { 
            List<Tag>? _subTags = EquInfoManage.GetTag(socketClientInfo.EQU, subTags);
            if (_subTags != null && _subTags.Count == subTags.Count)
            {
                foreach (var _subTag in _subTags)
                {
                    if (SubTags.TryGetValue(_subTag, out List<string>? clientIds))
                    {
                        if (!clientIds.Contains(socketClientInfo.ClientId))
                        {
                            clientIds.Add(socketClientInfo.ClientId);
                        }
                    }
                    else
                    {
                        SubTags.TryAdd(_subTag, new List<string> { socketClientInfo.ClientId });
                    }
                }
                return _subTags;
            }
            return new List<Tag>(); 
        }
        /// <summary>
        /// 取消订阅
        /// </summary>
        /// <param name="pipelineServer"></param>
        /// <param name="id"></param>
        /// <param name="UnsubTags"></param>
        public static void UnsubTag(SocketClientInfo socketClientInfo, List<string> UnsubTags)
        {
            foreach (var unsubTag in SubTags.Where(p => UnsubTags.Contains(p.Key.TagName)))
            {
                unsubTag.Value.Remove(socketClientInfo.ClientId);
            }
        }


    }
}
