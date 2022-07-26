using SEC.DataFormatModel;
using System.Text;

namespace SEC.Drivers
{
    public static class DataConversion
    {
        /// <summary>
        /// 转换枚举值
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="_Enum"></param>
        /// <param name="addressType"></param>
        /// <returns></returns>
        public static bool ToEnumByte<T>(this string _Enum, out T? addressType)
        {
            Enum.TryParse(typeof(T), _Enum, out object? result);
            if (result != null)
            {
                addressType = (T)result;
                return true;
            }
            addressType = default(T);
            return false;
        }
        /// <summary>
        /// byte转换
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="datas"></param>
        /// <param name="length"></param>
        /// <param name="translator"></param>
        /// <returns></returns>
        public static OperateResult<T[]> Translator<T>(this byte[]? datas, ushort length, Func<byte[], int, T> translator)
        {
            try
            {
                if (datas != null && datas.Any() && datas.Length / length > 0)
                {
                    datas = datas.Take(datas.Length - datas.Length % length).ToArray();
                    T[] result = new T[datas.Length / length];
                    for (int i = 0; i < result.Length; i++)
                    {
                        result[i] = translator(datas, i * length);
                    }
                    return new OperateResult<T[]> { IsSuccess = true, Content = result };
                }
                else
                {
                    return new OperateResult<T[]> { IsSuccess = false, Message = "读取数据为空" };
                }
            }
            catch (Exception e)
            {
                return new OperateResult<T[]> { IsSuccess = false, Message = $"读取数据发生异常{e.Message}" };
            }
        }
        /// <summary>
        /// 字符串类型转换
        /// </summary>
        /// <param name="datas"></param>
        /// <param name="strLength"></param>
        /// <param name="length"></param>
        /// <param name="encoding"></param>
        /// <returns></returns>
        public static OperateResult<string[]> Translator(this byte[]? datas, ushort strLength, ushort length, Encoding encoding)
        {
            try
            {
                if (datas != null && length > 0)
                {
                    string[] result = new string[length];
                    for (int i = 0; i < length; i++)
                    {
                        result[i] = encoding.GetString(datas, i * strLength * 2, strLength * 2);
                    }
                    return new OperateResult<string[]> { IsSuccess = true, Content = result };
                }
                else
                {
                    return new OperateResult<string[]> { IsSuccess = false, Message = "读取数据为空" };
                }
            }
            catch (Exception e)
            {
                return new OperateResult<string[]> { IsSuccess = false, Message = $"读取数据发生异常{e.Message}" };
            }
        }
        /// <summary>
        /// 对比两组byte
        /// </summary>
        /// <param name="data"></param>
        /// <param name="_byte"></param>
        /// <param name="startIndex"></param>
        /// <returns></returns>
        public static bool EqualsBytes(this byte[] data, byte[] _byte, int startIndex = 0)
        {
            if (data.Length < startIndex + _byte.Length) return false;
            for (int i = 0; i < _byte.Length; i++)
            {
                if (_byte[i] != data[i + startIndex])
                {
                    return false;
                }
            }
            return true;
        }
        /// <summary>
        /// 数据顺序
        /// </summary>
        /// <param name="data"></param>
        /// <param name="sequenceType"></param>
        /// <returns></returns>
        public static byte[]? DataSequence(this byte[]? data, string sequenceType)
        {
            if (data == null) return null;
            switch (sequenceType.ToUpper())
            {
                case "ABCD":
                default:
                    return data;
                case "BADC":
                    return data.HiloExchange();
                case "DCBA":
                    return data.Reverse().ToArray();
                case "CDAB":
                    return data.HiloExchange()?.Reverse().ToArray();
            }
        }
        /// <summary>
        /// 高低位互换
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static byte[]? HiloExchange(this byte[]? data)
        {
            if (data == null) return null;
            byte[] resultByt = new byte[data.Length - data.Length % 2];
            for (int i = 0; i < resultByt.Length; i += 2)
            {
                resultByt[i] = data[i + 1];
                resultByt[i + 1] = data[i];
            }
            return data.ToArray();
        } 
    }
}