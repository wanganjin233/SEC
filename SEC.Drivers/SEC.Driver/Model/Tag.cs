using SEC.Util;
using System.Text;

namespace SEC.Driver
{
    public class Tag : TagInfo
    {
        /// <summary>
        /// 使用驱动
        /// </summary>
        protected internal BaseDriver? baseDriver { get; set; }
        /// <summary>
        /// 原始值数据
        /// </summary>
        private byte[]? OriginalData;
        /// <summary>
        /// 值
        /// </summary>
        private object? _Value;
        /// <summary>
        /// 变量值
        /// </summary>
        public object? Value
        {
            get
            {
                return _Value;
            }
            set
            {
                try
                {
                    if (value != null)
                    {
                        if (value is string strValue)
                        {
                            byte[] bytes = DataType switch
                            {
                                TagTypeEnum.Boole => BitConverter.GetBytes(Convert.ToBoolean(strValue)),
                                TagTypeEnum.Ushort => BitConverter.GetBytes(Convert.ToUInt16(strValue)),
                                TagTypeEnum.Short => BitConverter.GetBytes(Convert.ToInt16(strValue)),
                                TagTypeEnum.Uint => BitConverter.GetBytes(Convert.ToUInt32(strValue)),
                                TagTypeEnum.Int => BitConverter.GetBytes(Convert.ToInt32(strValue)),
                                TagTypeEnum.Float => BitConverter.GetBytes(Convert.ToSingle(strValue)),
                                TagTypeEnum.Double => BitConverter.GetBytes(Convert.ToDouble(strValue)),
                                TagTypeEnum.Ulong => BitConverter.GetBytes(Convert.ToUInt64(strValue)),
                                TagTypeEnum.Long => BitConverter.GetBytes(Convert.ToInt64(strValue)),
                                TagTypeEnum.String => Encoding.GetEncoding(Coding).GetBytes(strValue.PadRight(DataLength, '\0')),
                                _ => throw new NotImplementedException("无法找到合适的转换")
                            };
                            baseDriver?.Write(this, bytes.DataSequence(Sort));
                        }
                        else
                        {
                            byte[] bytes = DataType switch
                            {
                                TagTypeEnum.Boole => BitConverter.GetBytes((bool)value),
                                TagTypeEnum.Ushort => BitConverter.GetBytes((ushort)value),
                                TagTypeEnum.Short => BitConverter.GetBytes((short)value),
                                TagTypeEnum.Uint => BitConverter.GetBytes((uint)value),
                                TagTypeEnum.Int => BitConverter.GetBytes((int)value),
                                TagTypeEnum.Float => BitConverter.GetBytes((float)value),
                                TagTypeEnum.Double => BitConverter.GetBytes((double)value),
                                TagTypeEnum.Ulong => BitConverter.GetBytes((ulong)value),
                                TagTypeEnum.Long => BitConverter.GetBytes((long)value),
                                TagTypeEnum.String => Encoding.GetEncoding(Coding).GetBytes((value.ToString() ?? string.Empty).PadRight(DataLength, '\0')),
                                _ => throw new NotImplementedException("无法找到合适的转换")
                            };
                            baseDriver?.Write(this, bytes.DataSequence(Sort));
                        }

                    }
                }
                catch (Exception e)
                {

                }
            }
        }
        /// <summary>
        /// 供驱动更新点位方法
        /// </summary>
        protected internal byte[]? UpdateValue
        {
            set
            {
                try
                {
                    if (value != null)
                    {
                        timestamp = DateTime.Now;
                    }
                    if ((value != null || OriginalData != null)
                        && (!OriginalData?.Equalsbytes(value) ?? true))
                    {
                        OriginalData = value;
                        byte[]? itemValue = value?.DataSequence(Sort);
                        oldValue = _Value;
                        _Value = itemValue == null ? null : DataType switch
                        {
                            TagTypeEnum.Boole => BitConverter.ToBoolean(itemValue),
                            TagTypeEnum.Ushort => BitConverter.ToUInt16(itemValue),
                            TagTypeEnum.Short => BitConverter.ToInt16(itemValue),
                            TagTypeEnum.Uint => BitConverter.ToUInt32(itemValue),
                            TagTypeEnum.Int => BitConverter.ToInt32(itemValue),
                            TagTypeEnum.Float => BitConverter.ToSingle(itemValue),
                            TagTypeEnum.Double => BitConverter.ToDouble(itemValue),
                            TagTypeEnum.Ulong => BitConverter.ToUInt64(itemValue),
                            TagTypeEnum.Long => BitConverter.ToInt64(itemValue),
                            TagTypeEnum.String => Encoding.GetEncoding(Coding).GetString(itemValue).Replace("\0", ""),
                            _ => throw new NotImplementedException("无法找到合适的转换")
                        };
                        ValueChangeEvent?.Invoke(this);
                    }
                }
                catch (Exception e)
                {

                }
            }
        }
        private DateTime timestamp = DateTime.MinValue;
        /// <summary>
        /// 时间戳
        /// </summary>
        public DateTime Timestamp => timestamp;
        /// <summary>
        /// 质量戳
        /// </summary>
        public QualityTypeEnum Quality
        {
            get
            {
                if (_Value == null)
                {
                    return QualityTypeEnum.Bad;
                }
                else if (timestamp.AddMilliseconds(Cycle) > DateTime.Now)
                {
                    return QualityTypeEnum.Good;
                }
                else
                {
                    return QualityTypeEnum.TimeOut;
                }
            }
        }
        /// <summary>
        /// 私有旧值
        /// </summary>
        private object? oldValue;
        /// <summary>
        /// 上次值
        /// </summary>
        public object? OldValue => oldValue;

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

    }
}
