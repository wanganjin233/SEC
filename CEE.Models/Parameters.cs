namespace CEE.Models
{
    /// <summary>
    /// 参数
    /// </summary>
    public class Parameter
    {
        /// <summary>
        /// 名称
        /// </summary>
        public string? Name { get; set; }
        /// <summary>
        /// 值
        /// </summary>
        public string? Value { get; set; }
        /// <summary>
        /// 编码
        /// </summary>
        public string? Code { get; set; }
        /// <summary>
        /// 最大值
        /// </summary>
        public string? MaxValue { get; set; }
        /// <summary>
        /// 最小值
        /// </summary>
        public string? MinValue { get; set; }
        /// <summary>
        /// 单位
        /// </summary>
        public string? Units { get; set; }
        /// <summary>
        /// 备注
        /// </summary>
        public string? Remark { get; set; }
    } 
}