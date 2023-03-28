using Dapper.Contrib.Extensions;

namespace SEC.Docker.Core
{
    [Table("EquState")]
    public class EquState
    {
        /// <summary>
        /// ID
        /// </summary>
        [Key]
        public string Id { get; set; } = string.Empty;
        /// <summary>
        /// 设备号
        /// </summary> 
        public string Equ { get; set; } = string.Empty;
        /// <summary>
        /// 设备名称
        /// </summary>
        public string EquName { get; set; } = string.Empty;
        /// <summary>
        /// 是否启用
        /// </summary>
        public bool Enable { get; set; }
        /// <summary>
        /// 任务id
        /// </summary>
        public string TaskId { get; set; } = string.Empty;
        /// <summary>
        /// 上传时间
        /// </summary>
        public DateTime UpdateTime { get; set; }
    }
}
