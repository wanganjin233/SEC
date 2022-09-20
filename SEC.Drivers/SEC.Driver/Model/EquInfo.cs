 namespace SEC.Driver
{
    public class EquInfo
    {
        /// <summary>
        /// 名称
        /// </summary>
        public string? Name { get; set; }
        /// <summary>
        /// 设备号
        /// </summary>
        public string? EQU { get; set; }
        /// <summary>
        /// 驱动类型
        /// </summary>
        public string? DriverType { get; set; }
        /// <summary>
        /// 连接字符串
        /// </summary>
        public string? ConnectionString { get; set; }
        /// <summary>
        /// 扫描周期
        /// </summary>
        public int ScanRate { get; set; } = 100;
        /// <summary>
        /// 所有点位
        /// </summary>
        public List<Tag> Tags { get; set; } = new List<Tag>(); 
    }

}
