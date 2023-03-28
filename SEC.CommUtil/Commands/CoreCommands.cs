using SEC.Models.Driver;
using SEC.Models.Interactive;
using SEC.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SEC.CommUtil
{
    public static class CoreCommands
    {
        /// <summary>
        /// 点位变化
        /// </summary>
        /// <param name="sockerClient"></param>
        /// <param name="tag"></param>
        public static void TagEvent(SocketClientHelper sockerClient, Tag tag)
        {
            sockerClient.Send(new OperateResult<Tag>()
            {
                Router = CoreRouters.TagChange,
                Content = tag
            });
        } 
        /// <summary>
        /// 订阅Tag点
        /// </summary>
        /// <param name="sockerClient"></param>
        /// <param name="TagNames"></param>
        /// <returns></returns>
        public static List<Tag> SubTags(SocketClientHelper sockerClient, List<string> TagNames)
        {
            return SubTagManage.Subs(sockerClient, TagNames, CoreRouters.SubTag);
        }
        /// <summary>
        /// 订阅Mq点
        /// </summary>
        /// <param name="sockerClient"></param>
        /// <param name="TagNames"></param>
        /// <returns></returns>
        public static List<Tag> SubMQs(SocketClientHelper sockerClient, List<string> TagNames)
        {
            return SubTagManage.Subs(sockerClient, TagNames, CoreRouters.SubMQ);
        }
        /// <summary>
        /// 订阅Mq点
        /// </summary>
        /// <param name="sockerClient"></param>
        /// <param name="TagNames"></param>
        /// <returns></returns>
        public static bool PubMQ(SocketClientHelper sockerClient, string route, string data)
        {
            return sockerClient.SendWait<string, bool>(new OperateResult<string>()
            {
                Router = CoreRouters.PubMQ,
                Content = $"{route}:{data}"
            })?.Content ?? false;
        }
        /// <summary>
        /// 推送日志
        /// </summary>
        /// <param name="sockerClient"></param>
        /// <param name="logInfo"></param>
        public static void Log(SocketClientHelper sockerClient, LogInfo logInfo)
        {
            sockerClient.Send(new OperateResult<LogInfo>()
            {
                Router = CoreRouters.Log,
                Content = logInfo
            });
        }
    }
}
