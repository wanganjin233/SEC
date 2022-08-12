using SEC.Driver;
using SEC.Util;

namespace SEC.Driver.MC3E
{
    public static class DriverExtend
    {
        /// <summary>
        /// 三菱MC读取结果协议头
        /// </summary> 
        private static byte[] resultCode = new byte[7] { 0xD0, 0x0, 0x0, 0xFF, 0xFF, 0x03, 0x0 };
        /// <summary>
        /// 校验数据并获取包
        /// </summary>
        /// <param name="_byte"></param>
        /// <returns></returns>
        public static byte[]? GetBody(this byte[]? _byte, bool isBit, int Length)
        {
            if (_byte != null && _byte.Length >= 9 && _byte.Equalsbytes(resultCode) && _byte.Length - 9 == BitConverter.ToUInt16(_byte, 7))
            {
                _byte = _byte.Skip(11).ToArray();
                if (isBit && _byte != null && _byte.Any())
                {
                    byte[] result = new byte[Length];
                    for (int i = 0; i < Length; i++)
                    {
                        result[i] = (byte)(_byte[i / 2] >> (i % 2 == 0 ? 4 : 0) & 1);
                    }
                    return result;
                }
                return _byte;
            }
            return null;
        }
    }
}
