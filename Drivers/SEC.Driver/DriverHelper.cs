using System.Text;

namespace SEC.Driver
{
    /// <summary>
    /// 驱动辅助类对象，提供了的读写操作的基本支持
    /// </summary>
    public static class DriverHelper
    {
        /// <summary>
        /// 批量读取为Bool类型
        /// </summary>
        /// <param name="driver"></param>
        /// <param name="startAddress"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public static OperateResult<bool[]> ReadBoole(this BaseDriver driver, string startAddress, ushort length = 1)
        {
            return driver.Read(startAddress, length, true).Translator(1, (byte[] m, int startIndex) => BitConverter.ToBoolean(m, startIndex));
        }
        /// <summary>
        /// 批量读取为Ushort类型
        /// </summary>
        /// <param name="startAddress"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public static OperateResult<ushort[]> ReadUshort(this BaseDriver driver, string startAddress, ushort length = 1)
        {
            return driver.Read(startAddress, (ushort)(length * driver.WordLength)).Translator(2, (byte[] m, int startIndex) => BitConverter.ToUInt16(m, startIndex));
        }
        /// <summary>
        /// 批量读取为short类型
        /// </summary>
        /// <param name="startAddress"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public static OperateResult<short[]> ReadShort(this BaseDriver driver, string startAddress, ushort length = 1)
        {
            return driver.Read(startAddress, (ushort)(length * driver.WordLength)).Translator(2, (byte[] m, int startIndex) => BitConverter.ToInt16(m, startIndex));
        }
        /// <summary>
        /// 批量读取为Uint类型
        /// </summary>
        /// <param name="startAddress"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public static OperateResult<uint[]> ReadUint(this BaseDriver driver, string startAddress, ushort length = 1)
        {
            return driver.Read(startAddress, (ushort)(length * driver.WordLength * 2)).Translator(4, (byte[] m, int startIndex) => BitConverter.ToUInt32(m, startIndex));
        }
        /// <summary>
        /// 批量读取为Int类型
        /// </summary>
        /// <param name="startAddress"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public static OperateResult<int[]> ReadInt(this BaseDriver driver, string startAddress, ushort length = 1)
        {
            return driver.Read(startAddress, (ushort)(length * driver.WordLength * 2)).Translator(4, (byte[] m, int startIndex) => BitConverter.ToInt32(m, startIndex));
        }
        /// <summary>
        /// 批量读取为Float类型
        /// </summary>
        /// <param name="startAddress"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public static OperateResult<float[]> ReadFloat(this BaseDriver driver, string startAddress, ushort length = 1)
        {
            return driver.Read(startAddress, (ushort)(length * driver.WordLength * 2)).Translator(4, (byte[] m, int startIndex) => BitConverter.ToSingle(m, startIndex));
        }
        /// <summary>
        /// 批量读取为ReadDouble类型
        /// </summary>
        /// <param name="startAddress"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public static OperateResult<double[]> ReadDouble(this BaseDriver driver, string startAddress, ushort length = 1)
        {
            return driver.Read(startAddress, (ushort)(length * driver.WordLength * 4)).Translator(8, (byte[] m, int startIndex) => BitConverter.ToDouble(m, startIndex));
        }
        /// <summary>
        /// 批量读取为long类型
        /// </summary>
        /// <param name="startAddress"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public static OperateResult<long[]> ReadLong(this BaseDriver driver, string startAddress, ushort length = 1)
        {
            return driver.Read(startAddress, (ushort)(length * driver.WordLength * 4)).Translator(8, (byte[] m, int startIndex) => BitConverter.ToInt64(m, startIndex));
        }
        /// <summary>
        /// 批量读取为Ulong类型
        /// </summary>
        /// <param name="startAddress"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public static OperateResult<ulong[]> ReadUlong(this BaseDriver driver, string startAddress, ushort length = 1)
        {
            return driver.Read(startAddress, (ushort)(length * driver.WordLength * 4)).Translator(8, (byte[] m, int startIndex) => BitConverter.ToUInt64(m, startIndex));
        }
        /// <summary>
        /// 批量读取为String类型
        /// </summary>
        /// <param name="startAddress"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public static OperateResult<string[]> ReadString(this BaseDriver driver, string startAddress, ushort strLength, Encoding encoding, ushort length = 1)
        {
            return driver.Read(startAddress, (ushort)(length * driver.WordLength * strLength)).Translator(strLength, length, encoding);
        }
        /// <summary>
        /// 写值
        /// </summary>
        /// <param name="driver"></param>
        /// <param name="startAddress"></param>
        /// <param name="obj"></param>
        /// <param name="encoding"></param>
        /// <returns></returns>
        public static OperateResult<bool> WriteObject(this BaseDriver driver, string startAddress, object obj, Encoding? encoding = null)
        {
            try
            {
                byte[]? bytes = null;
                bool isBit = false;
                if (obj is bool)
                {
                    bytes = BitConverter.GetBytes((bool)obj);
                    isBit = true;
                }
                else if (obj is short)
                {
                    bytes = BitConverter.GetBytes((short)obj);
                }
                else if (obj is ushort)
                {
                    bytes = BitConverter.GetBytes((ushort)obj);
                }
                else if (obj is int)
                {
                    bytes = BitConverter.GetBytes((int)obj);
                }
                else if (obj is uint)
                {
                    bytes = BitConverter.GetBytes((uint)obj);
                }
                else if (obj is long)
                {
                    bytes = BitConverter.GetBytes((long)obj);

                }
                else if (obj is ulong)
                {
                    bytes = BitConverter.GetBytes((ulong)obj);
                }
                else if (obj is float)
                {
                    bytes = BitConverter.GetBytes((float)obj);
                }
                else if (obj is double)
                {
                    bytes = BitConverter.GetBytes((double)obj);
                }
                else if (encoding != null && obj is string)
                {
                    bytes = encoding.GetBytes((string)obj);
                    if (bytes.Length % 2 == 1) bytes = bytes.Append<byte>(0).ToArray();
                }
                if (bytes == null)
                {
                    return new OperateResult<bool>() { IsSuccess = false, Message = "数据没有合适的转换" };
                }
                else if (driver.Write(startAddress, bytes, isBit))
                {
                    return new OperateResult<bool>() { IsSuccess = true };
                }
                else
                {
                    return new OperateResult<bool>() { IsSuccess = false, Message = "写入PLC异常" };
                }
            }
            catch (Exception e)
            {
                return new OperateResult<bool>() { IsSuccess = false, Message = $"数据转换异常:{e.Message}" };
            }
        }
    }
}
