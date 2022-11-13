using System;
using System.Linq;
using System.Text;

namespace SEC.Util
{
    /// <summary>
    /// 拓展类
    /// </summary>
    public static partial class Extention
    {
        /// <summary>
        /// 将文件转化位byte数组
        /// </summary>
        /// <param name="Path">文件地址</param>
        /// <returns>转换为的byte数组</returns>
        public static byte[] FileBytes(this string Path)
        {
            if (!File.Exists(Path))
            {
                return Array.Empty<byte>();
            }
            FileInfo fi = new FileInfo(Path);
            byte[] buff = new byte[fi.Length];
            FileStream fs = fi.OpenRead();
            fs.Read(buff, 0, Convert.ToInt32(fs.Length));
            fs.Close();
            return buff;
        }
    }
}
