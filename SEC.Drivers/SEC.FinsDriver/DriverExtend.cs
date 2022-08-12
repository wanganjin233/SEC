using SEC.Util;

namespace SEC.Driver.Fins
{
    public static class DriverExtend
    {
        //46 49 4E 53 00 00 00 18 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 01 01 00 00 00 00
        //46 49 4E 53 00 00 00 10 00 00 00 00 00 00 00 00 00 00 00 01 00 00 00 01
        /// <summary>
        /// 欧姆龙读取结果协议头
        /// </summary> 
        private static byte[] resultCode = new byte[6] { 0x46, 0x49, 0x4E, 0x53, 0x0, 0x0 };
        /// <summary>
        /// 登录成功标识
        /// </summary>
        private static byte[] loginSuccess = new byte[4] { 0, 0, 0, 0 };
        /// <summary>
        /// 读取成功标识
        /// </summary>
        private static byte[] readSuccess = new byte[2] { 0, 0 };
        /// <summary>
        /// 校验数据并获取包
        /// </summary>
        /// <param name="_byte"></param>
        /// <returns></returns>
        public static byte[]? GetLogIn(this byte[]? _byte)
        {
            if (_byte != null
                && _byte.Length >= 9
                && _byte.Equalsbytes(resultCode, 0)
                && _byte.Equalsbytes(loginSuccess, 14)
                && _byte.Length - 8 == BitConverter.ToUInt16(_byte.Reverse().ToArray(), 16))
            {
                return _byte.Skip(16).ToArray();
            }
            return null;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="_byte"></param>
        /// <returns></returns>
        public static byte[]? GetBody(this byte[]? _byte)
        {
            if (_byte != null && _byte.Length >= 30 && _byte.Equalsbytes(readSuccess, 28))
            {
                ushort commLength = BitConverter.ToUInt16(_byte.Skip(6).Take(2).Reverse().ToArray());
                if (_byte.Length - commLength == 8)
                {
                    _byte = _byte.Skip(30).ToArray();
                    return _byte;
                }
            }
            return null;
        }
    }
}
