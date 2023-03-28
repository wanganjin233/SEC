using Dapper.Contrib.Extensions;

namespace SEC.Docker.Core
{
    [Table("EquConfig")]
    public class EquConfig
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
        /// 配置存储路径
        /// </summary>
        public string ConfigPath { get; set; } = string.Empty;
        /// <summary>
        /// 版本号
        /// </summary>
        public string Version { get; set; } = string.Empty;
        /// <summary>
        /// 更新时间
        /// </summary>
        public DateTime UpdateTime { get; set; }
    }
}
