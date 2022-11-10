using SEC.Driver;
using SEC.Util;
using System.Text;

namespace SEC.DriverService
{
    public static class Extention
    {
        public static bool Send(this SocketClient socketClient, string msg)
        {
            byte[] bytes = socketClient.HeadBytes.AddBytes(Encoding.UTF8.GetBytes(msg)).AddBytes(socketClient.EndBytes);
            return socketClient.Send(bytes);
        }
    }
}
