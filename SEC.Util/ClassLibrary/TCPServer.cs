using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace SEC.Util
{
    public class TCPServer
    {
        /// <summary>
        /// 最大连接数
        /// </summary>
        private int MaxConnect { get; set; }
        /// <summary>
        /// 心跳时间
        /// </summary>
        private int HeartbeatTime { get; set; }
        /// <summary>
        /// 客户端列表
        /// </summary>
        protected ConcurrentDictionary<string, ListenSocket> ListenSocketDic = new();
        /// <summary>
        /// 创建服务
        /// </summary>
        /// <param name="port">端口</param>
        /// <param name="ip">地址</param>
        /// <param name="maxConnect">最大连接数</param>
        public TCPServer(int port, string ip = "127.0.0.1", int maxConnect = 10, int heartbeatTime = 10000)
        {
            HeartbeatTime = heartbeatTime;
            MaxConnect = maxConnect;
            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IPEndPoint IEP = new IPEndPoint(IPAddress.Parse(ip), port);
            socket.Bind(IEP);
            socket.Listen(10);
            socket.BeginAccept(AcceptCallback, socket);
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
        private void AddClient(Socket client)
        {
            ListenSocket listenSocket = new ListenSocket(client);
            if (string.IsNullOrEmpty(listenSocket.RemoteEndPoint) == false)
            {
                if (ListenSocketDic.Count >= MaxConnect)
                {
                    client.Send(Encoding.UTF8.GetBytes("超出服务器连接数上限"));
                    client.Close();
                    client.Dispose();
                }
                else
                {
                    client.ReceiveTimeout = HeartbeatTime;
                    client.SendTimeout = 1000;
                    ListenSocketDic.AddOrUpdate(listenSocket.RemoteEndPoint, listenSocket, (k, v) =>
                    {
                        RemoveClient(v);
                        v = listenSocket;
                        return v;
                    });
                    Receive(listenSocket);
                }
            }
        }
        /// <summary>
        /// 踢出会话
        /// </summary>
        /// <param name="client"></param>
        private void RemoveClient(ListenSocket client)
        {
            if (ListenSocketDic.Remove(client.RemoteEndPoint, out ListenSocket? listen))
            {
                listen.Dispose();
            }
        }
        /// <summary>
        /// 接收委托
        /// </summary>
        /// <param name="client"></param>
        /// <param name="bytes"></param>
        public delegate void ReceiveDelegate(ListenSocket listenSocket, byte[] bytes);
        /// <summary>
        /// 接收事件
        /// </summary>
        public event ReceiveDelegate? ReceiveEvent;
        /// <summary>
        /// 头字节
        /// </summary>
        public byte[] HeadBytes { get; set; } = new byte[1] { 2 };
        /// <summary>
        /// 尾字节
        /// </summary>
        public byte[] EndBytes { get; set; } = new byte[1] { 3 };
        /// <summary>
        /// 数据长度位置
        /// </summary>
        public int DataLengthLocation { get; set; } = 0;
        /// <summary>
        /// 数据长度类型
        /// </summary>
        public int DataLengthType { get; set; } = 1;
        /// <summary>
        /// 接收字节
        /// </summary>
        /// <param name="listenSocket"></param>
        private void Receive(ListenSocket listenSocket)
        {
            _ = Task.Run(() =>
            {
                //缓存
                byte[] buffer = new byte[listenSocket.Socket.ReceiveBufferSize];
                //缓存下标
                int index = 0;
                while (true)
                {
                    try
                    {
                        int startIndex = -1;
                        int length = -1;
                        do
                        {
                            //接收阻塞
                            int count = listenSocket.Socket.Receive(listenSocket.ReceiveBuffer);
                            //断开信号
                            if (count == 0)
                            {
                                RemoveClient(listenSocket);
                                return;
                            }
                            //判断接收大于最大缓存
                            if (index + count < listenSocket.Socket.ReceiveBufferSize)
                            {
                                listenSocket.ReceiveBuffer.Take(count).ToArray().CopyTo(buffer, index);
                                index += count;
                            }
                            else
                            {
                                //大于最大缓退出
                                break;
                            }
                            if (startIndex == -1)
                            {
                                startIndex = buffer.IndexOf(HeadBytes);
                                if (startIndex != -1 && DataLengthLocation != -1)
                                {
                                    length = DataLengthType switch
                                    {
                                        1 => buffer[DataLengthLocation],
                                        _ => BitConverter.ToUInt16(buffer, DataLengthLocation)
                                    };
                                }
                            }
                            //接收信息小于头或者未找到尾时循环
                        } while (index < HeadBytes.Length
                                || (startIndex > buffer.IndexOf(EndBytes)
                                && (index - startIndex - 1 != length + HeadBytes.Length + EndBytes.Length
                                    || DataLengthLocation == -1)));

                        //获取有效长度bytes
                        var _buffer = buffer.Take(index).ToArray();
                        //根据头尾切分
                        var buffers = _buffer.Capture(HeadBytes, EndBytes);

                        if (buffers.Any())
                        {
                            //根据切分遍历触发接收事件发送bytes
                            foreach (var bufferitem in buffers)
                            {
                                ThreadPool.QueueUserWorkItem(p =>
                                    ReceiveEvent?.Invoke(listenSocket, bufferitem.Skip(DataLengthLocation == -1 ? 0 : DataLengthLocation + DataLengthType).ToArray())
                                );
                            }
                            //获取尾下标
                            int lastIndex = _buffer.LastIndexOf(EndBytes) + EndBytes.Length;
                            //获取尾数据
                            var lastBuffer = _buffer.Skip(lastIndex).ToArray();
                            //初始化缓存
                            buffer = new byte[listenSocket.Socket.ReceiveBufferSize];
                            //尾数据拷贝至缓存
                            lastBuffer.CopyTo(buffer, 0);
                            //设置缓存下标
                            index = lastBuffer.Length;

                        }
                        else //无法切分触发接收事件发送所有bytes
                        {
                            ThreadPool.QueueUserWorkItem(p => ReceiveEvent?.Invoke(listenSocket, _buffer));
                            //缓存和下标复位
                            buffer = new byte[listenSocket.Socket.ReceiveBufferSize];
                            index = 0;
                        }
                    }
                    catch (Exception)
                    {
                        RemoveClient(listenSocket);
                        return;
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
        public bool Send(string clientId, byte[] bytes)
        {
            if (ListenSocketDic.TryGetValue(clientId, out ListenSocket? listenSocket))
            {
                return listenSocket.Socket.Send(bytes) == bytes.Length;
            }
            return false;
        }
        /// <summary>
        /// 发送到所有客户端
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns>客户端对象和发送状态</returns>
        public Dictionary<Socket, bool> Send(byte[] bytes)
        {
            List<Task<KeyValuePair<Socket, bool>>> sendTasks = new();
            foreach (var listenSocket in ListenSocketDic.Values)
            {
                Task<KeyValuePair<Socket, bool>> sendTask = SendAsync(listenSocket, bytes, SocketFlags.None);
                sendTasks.Add(sendTask);
            }
            return sendTasks.ToDictionary(p => p.Result.Key, o => o.Result.Value);
        }
        /// <summary>
        /// 异步发送
        /// </summary>
        /// <param name="listenSocket"></param>
        /// <param name="bytes"></param>
        /// <param name="socketFlags"></param>
        /// <returns></returns>
        private async Task<KeyValuePair<Socket, bool>> SendAsync(ListenSocket listenSocket, byte[] bytes, SocketFlags socketFlags)
        {
            int count = await listenSocket.Socket.SendAsync(bytes, SocketFlags.Broadcast);
            return new KeyValuePair<Socket, bool>(listenSocket.Socket, count == bytes.Length);
        }
    }
    /// <summary>
    /// 监听客户
    /// </summary>
    public class ListenSocket : IDisposable
    {
        /// <summary>
        /// 初始化监听的客户端
        /// </summary>
        /// <param name="socket">套接字</param>
        public ListenSocket(Socket socket)
        {
            Socket = socket;
            ReceiveBuffer = new byte[socket.ReceiveBufferSize];
        }
        /// <summary>
        /// 客户端地址端口
        /// </summary>
        public string RemoteEndPoint => Socket?.RemoteEndPoint?.ToString() ?? string.Empty;
        /// <summary>
        /// 客户端套接字
        /// </summary>
        public Socket Socket { get; set; }
        /// <summary>
        /// 缓冲区
        /// </summary>
        public byte[] ReceiveBuffer { get; set; }
        /// <summary>
        /// 释放
        /// </summary>
        public void Dispose()
        {
            Socket.Close();
            Socket.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
