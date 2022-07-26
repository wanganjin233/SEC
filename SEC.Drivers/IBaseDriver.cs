using SEC.DataFormatModel.TagInfo;

namespace SEC.Drivers
{
    /// <summary>
    /// 驱动接口
    /// </summary>
    public interface IBaseDriver 
    {
        /// <summary>
        /// 批量读取地址
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        public byte[]? Read(string address, ushort length, bool isBit = false);
        /// <summary>
        /// 写入地址
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        public bool Write(string address, byte[] Value, bool isBit = false);
        /// <summary>
        /// 异步读取地址
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        public Task<byte[]?> AsyncRead(string address, ushort length, bool isBit);
        /// <summary>
        /// 异步写入地址
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        public Task<bool> AsyncWrite(string address, byte[] Value, bool isBit); 
        /// <summary>
        /// 设备状态
        /// </summary>
        public bool DriverState { get; }

        /// <summary>
        /// 字长度
        /// </summary>
        public ushort WordLength
        {
            get;
        }
        public Dictionary<string, List<Tag>> AddressTransition(List<Tag> tagValues);
    }
}