using SEC.Enum.Driver;

namespace SEC.Models.Driver
{
    public class Tag
    { 
        /// <summary>
        /// 旧值
        /// </summary>
        public virtual object? OldValue { get; set; }
        /// <summary>
        /// 缩放后的值
        /// </summary>
        public virtual object? ZoomValue { get; set; }
        /// <summary>
        /// 变量值
        /// </summary>
        public virtual object? Value { get; set; }
        /// <summary>
        /// 站号
        /// </summary>
        public byte StationNumber { get; set; } = 1;
        /// <summary>
        /// 变量类型
        /// </summary>
        public TagTypeEnum DataType { get; set; } = TagTypeEnum.Short;
        /// <summary>
        /// 点位名称
        /// </summary>
        public string TagName { get; set; } = string.Empty;
        /// <summary>
        /// 完整地址
        /// </summary>
        public string Address { get; set; } = string.Empty;
        /// <summary>
        /// 位置
        /// </summary>
        public uint Location { get; set; }
        /// <summary>
        /// 类型
        /// </summary>
        public object Type { get; set; } = 0x0;
        /// <summary>
        /// 是否为位地址
        /// </summary>
        public bool IsBit { get; set; } = false;
        /// <summary>
        /// 位地址位置
        /// </summary>
        public int BitLocation { get; set; } = -1;

        private int dataLength = 0;
        /// <summary>
        /// 数据长度
        /// </summary>
        public int DataLength
        {
            get
            {
                return DataType switch
                {
                    TagTypeEnum.Boole => 1,
                    TagTypeEnum.Ushort => 2,
                    TagTypeEnum.Short => 2,
                    TagTypeEnum.Uint => 4,
                    TagTypeEnum.Int => 4,
                    TagTypeEnum.Float => 4,
                    TagTypeEnum.Double => 8,
                    TagTypeEnum.Ulong => 8,
                    TagTypeEnum.Long => 8,
                    TagTypeEnum.String => dataLength,
                    _ => dataLength
                };
            }
            set
            {
                dataLength = value;
            }
        }
        /// <summary>
        /// 缩放倍数
        /// </summary>
        public double Magnification { get; set; } = 1;
        /// <summary>
        /// 读写权限
        /// </summary>
        public string ClientAccess { get; set; } = "R/W";
        /// <summary>
        /// 单位
        /// </summary>
        public string? EngUnits { get; set; }
        /// <summary>
        /// 备注
        /// </summary>
        public string? Description { get; set; }
        /// <summary>
        /// 顺序
        /// </summary>
        public string Sort { get; set; } = "ABCD";
        /// <summary>
        /// 点位异常信息
        /// </summary>
        public string? TagErrMsg { get; set; }
        /// <summary>
        /// 编码
        /// </summary>
        public string Coding { get; set; } = "ASCII";
        /// <summary>
        /// 时间
        /// </summary>
        public virtual DateTime Timestamp { get; set; } 
        /// <summary>
        /// 质量戳
        /// </summary>
        public virtual QualityTypeEnum Quality { get; set; }
        #region 事件委托
        /// <summary>
        /// 值变化委托
        /// </summary>
        public delegate void ValueChangeDelegate(Tag tag);
        /// <summary>
        /// 值变化事件
        /// </summary>
        public event ValueChangeDelegate? ValueChangeEvent;
        #endregion
        public void SendValueChangeEvent()
        {
            ValueChangeEvent?.Invoke(this);
        }
    }
}
