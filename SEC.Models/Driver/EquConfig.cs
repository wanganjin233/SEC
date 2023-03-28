namespace SEC.Models.Driver
{
    public class EquConfig
    {
        /// <summary>
        /// 名称
        /// </summary>
        public string Name { get; set; } = string.Empty;
        /// <summary>
        /// 设备号
        /// </summary>
        public string EQU { get; set; } = string.Empty;
        /// <summary>
        /// 驱动类型
        /// </summary>
        public string DriverType { get; set; } = string.Empty;
        /// <summary>
        /// 处理类
        /// </summary>
        public List<string> Operations { get; set; } = new List<string>();
        /// <summary>
        /// 连接字符串
        /// </summary>
        public string ConnectionString { get; set; } = string.Empty;
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
