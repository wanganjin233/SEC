using SEC.Interface.Interactive;
using SEC.Models.Driver;

namespace SEC.CommUtil
{
    public class BaseController : ICommController
    {
        /// <summary>
        /// 订阅的tag点位变化
        /// </summary>
        /// <param name="tag"></param>
        public void SubTagChange(Tag tag)
        {
            if (SubTagManage.SubTagEvents.TryGetValue(tag.TagName, out var subTagEvents))
            {
                subTagEvents.Value = tag.Value;
                subTagEvents.SendValueChangeEvent();
            }
        }
        /// <summary>
        /// 关闭程序
        /// </summary>
        public bool Kill()
        {
            ThreadPool.QueueUserWorkItem(p =>
            { 
                Thread.Sleep(2000);
                Environment.Exit(0);
            });
            return true;
        }
    }
}
