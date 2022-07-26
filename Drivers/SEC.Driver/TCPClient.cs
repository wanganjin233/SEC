using System.Net.Sockets;

namespace SEC.Driver
{
    public class TCPClient : ICommunication, IDisposable
    {
        /// <summary>
        /// 连接套接字
        /// </summary> 
        private Socket? clientSocket;
        /// <summary>
        /// 服务端ip地址
        /// </summary>
        private readonly string ipAddress = string.Empty;
        public string IpAddress => ipAddress;
        /// <summary>
        /// 服务端端口
        /// </summary>
        private readonly int port;
        public int Port => port;
        /// <summary>
        /// 超时时间
        /// </summary>
        readonly int _TimeOut;
        /// <summary>
        /// 接收缓存
        /// </summary>
        private byte[] Buffer { get; set; }
        /// <summary>
        /// 连接状态
        /// </summary>
        public bool Connected
        {
            get
            {
                return clientSocket?.Connected ?? false; 
            }
        }
        /// <summary>
        /// 初始化连接
        /// </summary>
        /// <param name="timeOut"></param>
        /// <returns></returns> 
        public TCPClient(string ServerIP, int serverPort, int timeOut = 1000)
        {
            ipAddress = ServerIP;
            port = serverPort;
            _TimeOut = timeOut;
            if (!Connect())
            {
                throw new Exception("连接驱动失败");
            }; 
            Buffer = new byte[clientSocket.ReceiveBufferSize]; 
        }
        /// <summary>
        /// 重新连接
        /// </summary>
        /// <returns></returns>
        public bool ReConnect()
        {
            Close();
            return Connect();
        }
        /// <summary>
        /// 连接
        /// </summary> 
        /// <returns></returns>
        private bool Connect()
        {
            try
            { 
                if (!Connected)
                {
                    clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    clientSocket.SendTimeout = _TimeOut;
                    clientSocket.ReceiveTimeout = _TimeOut;
                    clientSocket.Connect(IpAddress, Port);
                } 
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }
        /// <summary>
        /// 接收
        /// </summary>
        /// <returns></returns>
        public byte[]? Receive()
        { 
            if (Connected)
            {
                try
                {
                    int len = clientSocket.Receive(Buffer);
                    if (len == 0)
                    {
                        Close();
                        return null;
                    }
                    return Buffer.Take(len).ToArray();
                }
                catch (Exception)
                {
                    return null;
                }
            }
            return null; 
        }
        /// <summary>
        /// 发送
        /// </summary>
        /// <param name="buffer"></param>
        /// <returns></returns>
        public bool Send(byte[] buffer)
        { 
            if (Connected)
            {
                try
                {
                    int len = clientSocket.Send(buffer);
                    return buffer.Length == len;
                }
                catch (Exception)
                {
                    return false;
                }
            }
            return false; 
        }
        /// <summary>
        /// 释放
        /// </summary>
        public void Dispose()
        {
            Close();
            GC.SuppressFinalize(this);
        }
        /// <summary>
        /// 关闭连接
        /// </summary>
        public void Close()
        { 
            if (Connected)
                clientSocket?.Disconnect(false);
            clientSocket?.Close();
            clientSocket?.Dispose();
            clientSocket = null;

        }
    }
}