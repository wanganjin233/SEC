namespace SEC.Util
{
    public interface ICommunication : IDisposable
    {
        /// <summary>
        /// 连接
        /// </summary>
        /// <returns></returns>
        public bool Connect();
        /// <summary>
        /// 关闭
        /// </summary>
        public void Close();
        /// <summary>
        /// 连接状态
        /// </summary>
        public bool Connected { get; }
        /// <summary>
        /// 接受信息
        /// </summary>
        /// <returns></returns>
        public byte[]? Receive();
        /// <summary>
        /// 发送信息
        /// </summary>
        /// <param name="buffer"></param>
        /// <returns></returns>
        public bool Send(byte[] buffer); 
    }
}
