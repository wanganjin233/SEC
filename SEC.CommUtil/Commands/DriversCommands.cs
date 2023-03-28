using SEC.CommUtil.Routers;
using SEC.Models.Driver;
using SEC.Models.Interactive;

namespace SEC.CommUtil
{
    public static class DriversCommands
    {
        /// <summary>
        /// 获取所有点位信息
        /// </summary>
        /// <param name="sockerClient"></param> 
        /// <returns></returns>
        public static List<Tag>? GetAllTag(SocketClientHelper sockerClient)
        {
            return sockerClient.SendWait<object, List<Tag>>(new OperateResult<object>()
            {
                Router = DriversRouters.GetAllTag,
                ReceiverIdentity = $"{sockerClient.ScokerCilentInfo.EQU}_SEC.DriversRun",
            })?.Content;
        }
        /// <summary>
        /// 写入点位
        /// </summary>
        /// <param name="sockerClient"></param>
        /// <param name="tag"></param>
        /// <returns></returns>
        public static bool WriteTag(SocketClientHelper sockerClient, Tag tag)
        {

            return sockerClient.SendWait<Tag, bool>(new OperateResult<Tag>()
            {
                Router = DriversRouters.WriteTag,
                ReceiverIdentity = $"{sockerClient.ScokerCilentInfo.EQU}_SEC.DriversRun",
                Content = tag
            })?.Content ?? false;
        }
    }
}
