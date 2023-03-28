using SEC.Models.Driver;
using SEC.Models.Interactive;
namespace SEC.CommUtil
{
    public static class BaseCommands
    {
        /// <summary>
        /// 推送tag
        /// </summary>
        /// <param name="Id"></param>
        /// <param name="sockerServer"></param>
        /// <param name="tag"></param>
        public static void TagPush(string Id, SocketServerHelper sockerServer, Tag tag)
        {
            string? id = sockerServer.ScokerCilentInfos.FirstOrDefault(p => p.Value.ClientId == Id).Key;
            if (id != null)
            {
                sockerServer.Send(id, new OperateResult<Tag>()
                {
                    Router = BaseRouters.SubTagChange,
                    Content = tag
                });
            };
        }
        /// <summary>
        /// 关闭某进程
        /// </summary>
        /// <param name="Id"></param>
        /// <param name="pipelineServer"></param>
        public static bool Kill(string Id, SocketServerHelper sockerServer)
        {
            return sockerServer.SendWait<string, bool>(Id, new OperateResult<string>()
            {
                Router = BaseRouters.Kill
            })?.Content ?? false;
        }

    }
}
