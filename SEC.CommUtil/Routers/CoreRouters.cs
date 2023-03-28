namespace SEC.CommUtil
{
    public static class CoreRouters
    {
        /// <summary>
        /// 日志上传路由
        /// </summary>
        public static string Log => "LogController/Log";
        /// <summary>
        /// 点位变化路由
        /// </summary>
        public static string TagChange => "TagController/TagChange";
        /// <summary>
        /// 订阅tag点路由
        /// </summary>
        public static string SubTag => "TagController/SubTags";
        /// <summary>
        /// 订阅Mq点路由
        /// </summary>
        public static string SubMQ => "MQController/SubMQs";
        /// <summary>
        /// 发送信息到Mq点
        /// </summary>
        public static string PubMQ => "MQController/PubMQ"; 
    }
}
