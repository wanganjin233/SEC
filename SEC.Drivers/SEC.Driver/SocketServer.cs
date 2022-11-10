using System.Net;
using System.Net.Sockets;
using System.Text;

namespace SEC.Driver
{
    public class SocketServer : ICommunicationInfo, IDisposable
    {
        /// <summary>
        /// 最大连接数
        /// </summary>
        private int MaxConnect { get; set; }
        /// <summary>
        /// 头字节
        /// </summary>
        public byte[] HeadBytes { get; set; } = Array.Empty<byte>();
        /// <summary>
        /// 尾字节
        /// </summary>
        public byte[] EndBytes { get; set; } = Array.Empty<byte>();
        /// <summary>
        /// 数据长度位置
        /// </summary>
        public int DataLengthLocation { get; set; } = -1;
        /// <summary>
        /// 数据长度类型
        /// </summary>
        public LengthTypeEnum DataLengthType { get; set; } = LengthTypeEnum.Byte; 
        /// <summary>
        /// 长度补充
        /// </summary>
        public int LengthReplenish { set; get; } = 0;
        /// <summary>
        /// 接收委托
        /// </summary>
        /// <param name="client"></param>
        /// <param name="bytes"></param>
        public delegate void ReceiveDelegate(Socket socket, byte[] bytes);
        /// <summary>
        /// 接收事件
        /// </summary>
        public event ReceiveDelegate? ReceiveEvent;
        /// <summary>
        /// 客户端列表
        /// </summary>
        protected List<Socket> Sockets = new();
        /// <summary>
        /// 发送超时时间
        /// </summary>
        public int SendTimeout { get; set; } = 1000;
        /// <summary>
        /// 接收超时时间
        /// </summary>
        public int ReceiveTimeout { get; set; } = 10000;
        private readonly Socket ServerSocket;
        /// <summary>
        /// 创建服务
        /// </summary>
        /// <param name="port">端口</param>
        /// <param name="ip">地址</param>
        /// <param name="maxConnect">最大连接数</param>
        public SocketServer(int port, int maxConnect = 10)
        {
            MaxConnect = maxConnect;
            ServerSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            ServerSocket.Bind(new IPEndPoint(IPAddress.Any, port));
            ServerSocket.Listen(maxConnect);
            ServerSocket.BeginAccept(AcceptCallback, ServerSocket);
        }
        /// <summary>
        /// 创建服务
        /// </summary>
        /// <param name="port">端口</param>
        /// <param name="ip">地址</param>
        /// <param name="maxConnect">最大连接数</param>
        public SocketServer(string unixPath, int maxConnect = 10)
        {
            MaxConnect = maxConnect;
            ServerSocket = new Socket(AddressFamily.Unix, SocketType.Stream, ProtocolType.IP);
            ServerSocket.Bind(new UnixEndPoint(unixPath));
            ServerSocket.Listen(maxConnect);
            ServerSocket.BeginAccept(AcceptCallback, ServerSocket);
        }
        /// <summary>
        /// 客户端连接回调
        /// </summary>
        /// <param name="ar"></param>
        private void AcceptCallback(IAsyncResult ar)
        {
            Socket? _Socket = ar.AsyncState as Socket;
            if (_Socket != null)
            {
                var client = _Socket.EndAccept(ar);
                AddClient(client);
                _Socket.BeginAccept(AcceptCallback, _Socket);
            }
        }
        /// <summary>
        /// 新增会话方法
        /// </summary>
        /// <param name="client"></param>
        private void AddClient(Socket socket)
        {
            if (Sockets.Count >= MaxConnect)
            {
                socket.Send(Encoding.UTF8.GetBytes("超出服务器连接数上限"));
                Console.WriteLine("超出服务器连接数上限");
                RemoveClient(socket);
            }
            else
            {
                socket.ReceiveTimeout = ReceiveTimeout;
                socket.SendTimeout = SendTimeout;
                Sockets.Add(socket);
                ReceiveLoop(socket);
            }
        }
        /// <summary>
        /// 踢出会话
        /// </summary>
        /// <param name="client"></param>
        private void RemoveClient(Socket socket)
        {
            if (Sockets.Remove(socket))
            {
                if (socket.Connected)
                    socket?.Disconnect(false);
                socket?.Close();
                socket?.Dispose();
                GC.Collect();
            }
        }
        /// <summary>
        /// 接收触发
        /// </summary>
        /// <returns></returns>
        public void ReceiveLoop(Socket socket)
        {
            byte[] ReceiveBuffer = Array.Empty<byte>();
            _ = Task.Run(() =>
            {
                //接收缓存
                while (true)
                {
                    var bytes = this.ReceiveProcess(socket, ReceiveBuffer);
                    if (bytes != null)
                    {
                        foreach (var bufferitem in bytes)
                        {
                            ThreadPool.QueueUserWorkItem(p =>
                                ReceiveEvent?.Invoke(socket, bufferitem)
                            );
                        }
                    }
                    else
                    {
                        RemoveClient(socket);
                    }
                }
            });
        }
        /// <summary>
        /// 发送到指定客户端
        /// </summary>
        /// <param name="clientId"></param>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public bool Send(Socket socket, byte[] bytes)
        { 
            bool success = false;
            try
            {
                if (socket != null && socket.Connected)
                {
                    int len = socket.Send(bytes);
                    success = bytes.Length == len;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            return success;
             
        }
        /// <summary>
        /// 发送到所有客户端
        /// </summary>
        /// <param name="bytes"></param> 
        public void Send(byte[] bytes)
        {
            foreach (var socket in Sockets)
            {
                ThreadPool.QueueUserWorkItem(p => Send(socket, bytes));
            }
        }
        /// <summary>
        /// 释放
        /// </summary>
        public void Dispose()
        {
            foreach (var socket in Sockets)
            {
                if (socket.Connected)
                    socket?.Disconnect(false);
                socket?.Close();
                socket?.Dispose();
            }
            if (ServerSocket.Connected)
                ServerSocket?.Disconnect(false);
            ServerSocket?.Close();
            ServerSocket?.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
