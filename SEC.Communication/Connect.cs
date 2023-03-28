using SEC.Interface.Driver;
using SEC.Util;
using System.Text.RegularExpressions;

namespace SEC.Communication
{
    public static class Connect
    {
        /// <summary>
        /// COM1,9600,8,无,1
        /// </summary>
        /// <param name="connectionStr"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static ICommunication? ConnectionResolution(string? connectionStr)
        {
            if (string.IsNullOrEmpty(connectionStr))
            {
                return null;
            }
            else if (Regex.Matches(connectionStr, "COM[0-9]+,[0-9]+,[0-9],.+,.+").Any())
            {
                string[] connectionStrSplit = connectionStr.Split(',');
                return new SerialPort(connectionStrSplit[0],
                                      connectionStrSplit[1].ToInt(),
                                      connectionStrSplit[2].ToInt(),
                                      connectionStrSplit[3],
                                      connectionStrSplit[4]);
            }
            else if (Regex.Matches(connectionStr, "[0-9]+[.][0-9]+[.][0-9]+[.][0-9]+[:][0-9]+").Any())
            {
                return new SocketClient(connectionStr);
            }
            throw new Exception("未找到合适的连接");
        }
    }
}
