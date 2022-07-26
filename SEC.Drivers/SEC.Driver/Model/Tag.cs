﻿using Newtonsoft.Json.Linq;
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
        private object? zoomValue;
        /// <summary>
        /// 缩放后的值
        /// </summary>
        public object? ZoomValue
        {
            get
            {
                return zoomValue;
            }
            set
            {
                if (value != null)
                {
                    if (DataType == TagTypeEnum.String || DataType == TagTypeEnum.Boole)
                    {
                        Value = value;
                    }
                    else
                    {
                        Value = BitConverter.GetBytes(Convert.ToDouble(value.ToString()) * Magnification);
                    }
                }
            }
        }
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
                        baseDriver?.Write(this, value);
                    }
                }
                catch (Exception e)
                {

                }
            }
        }
        public object? SetValue
        {
            set
            {
                _Value = value;
            }
        } 
        /// <summary>
        /// 更新点位值
        /// </summary>
        public byte[]? UpdateValue
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
                        zoomValue = DataType switch
                        {
                            TagTypeEnum.Ushort => (ushort?)_Value / Magnification,
                            TagTypeEnum.Short => (short?)_Value / Magnification,
                            TagTypeEnum.Uint => (uint?)_Value / Magnification,
                            TagTypeEnum.Int => (int?)_Value / Magnification,
                            TagTypeEnum.Ulong => (ulong?)_Value / Magnification,
                            TagTypeEnum.Long => (long?)_Value / Magnification,
                            TagTypeEnum.Float => (float?)_Value / Magnification,
                            TagTypeEnum.Double => (double?)_Value / Magnification,
                            _ => null
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
        public DateTime Timestamp { get => timestamp; set { timestamp = value; } }
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
                else if (timestamp.AddMilliseconds(10000) > DateTime.Now)
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
