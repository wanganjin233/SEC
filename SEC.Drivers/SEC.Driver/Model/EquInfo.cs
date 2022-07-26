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
        /// 所有点位
        /// </summary>
        public List<Tag> Tags { get; set; } = new List<Tag>(); 
    }

}
