using System.Net.Sockets;

namespace SEC.Driver
{
    public class SocketClient : ICommunication
    {
        /// <summary>
        /// 连接套接字
        /// </summary> 
        private Socket? clientSocket;
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
        /// 缓存
        /// </summary>
        private byte[] ReceiveBuffer { set; get; } = Array.Empty<byte>();
        /// <summary>
        /// 长度补充
        /// </summary>
        public int LengthReplenish { set; get; } = 0;
        /// <summary>
        /// 心跳定时器
        /// </summary>
        private Timer? Timer = null;
        /// <summary>
        /// 连接方法
        /// </summary>
        private readonly Func<Socket?> connFunc;
        /// <summary>
        /// 接收委托
        /// </summary>
        /// <param name="client"></param>
        /// <param name="bytes"></param>
        public delegate void ReceiveDelegate(Socket socket, byte[] bytes);
        /// <summary>
        /// 连接状态
        /// </summary>
        public bool Connected => clientSocket?.Connected ?? false;
        /// <summary>
        /// 发送超时时间
        /// </summary>
        public int SendTimeout { get; set; } = 0;
        /// <summary>
        /// 接收超时时间
        /// </summary>
        public int ReceiveTimeout { get; set; } = 0;
        /// <summary>
        /// 心跳周期
        /// </summary>
        public int HeartbeatCycle { get; set; } = 5000;
        /// <summary>
        /// 心跳包
        /// </summary>
        public byte[]? HeartbeatBytes = null;
        /// <summary>
        /// 登陆包
        /// </summary>
        public byte[]? LogBytes = null;
        /// <summary>
        /// 接收事件
        /// </summary>
        public event ReceiveDelegate? ReceiveEvent;
        /// <summary>
        /// 初始化连接
        /// </summary>
        /// <param name="ConnectionString">连接字符串</param>
        /// <param name="timeOut">超时时间</param>
        public SocketClient(string ConnectionString, bool unix = false)
        {
            if (unix)
            {
                connFunc = () =>
                {
                    try
                    {
                        Socket socket = new(AddressFamily.Unix, SocketType.Stream, ProtocolType.IP)
                        {
                            SendTimeout = SendTimeout,
                            ReceiveTimeout = ReceiveTimeout
                        };
                        socket.Connect(new UnixEndPoint(ConnectionString));
                        return socket;
                    }
                    catch (Exception e)
                    {

                    }
                    return null;
                };
            }
            else
            {
                connFunc = () =>
                {
                    try
                    {
                        string[] ConnectionStrings = ConnectionString.Split(":");
                        if (ConnectionStrings.Length == 2)
                        {
                            Socket socket = new(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
                            {
                                SendTimeout = SendTimeout,
                                ReceiveTimeout = ReceiveTimeout
                            };
                            socket.Connect(ConnectionStrings[0], int.Parse(ConnectionStrings[1]));
                            return socket;
                        }

                    }
                    catch (Exception e)
                    {

                    }
                    return null;
                };
            }
        }
        /// <summary>
        /// 连接
        /// </summary> 
        /// <returns></returns>
        public void Connect()
        {
            try
            {
                Close();
                if (!Connected)
                {
                    clientSocket = connFunc.Invoke();
                    if (ReceiveEvent != null)
                    {
                        ReceiveLoop();
                    }
                    if (LogBytes?.Length > 0)
                    {
                        Send(LogBytes);
                    }
                    if (HeartbeatBytes?.Length > 0)
                    {
                        Timer = new Timer((o) => Send(HeartbeatBytes), null, HeartbeatCycle, HeartbeatCycle);
                    }
                }
            }
            catch (Exception e)
            {


            }
        }
        /// <summary>
        /// 接收触发
        /// </summary>
        /// <returns></returns>
        public void ReceiveLoop()
        {
            _ = Task.Run(() =>
            {
                //接收缓存
                while (Connected && clientSocket != null)
                {
                    var bytes = this.ReceiveProcess(clientSocket, ReceiveBuffer);
                    if (bytes != null)
                    {
                        foreach (var bufferitem in bytes)
                        {
                            ThreadPool.QueueUserWorkItem(p =>
                                ReceiveEvent?.Invoke(clientSocket, bufferitem)
                            );
                        }
                    }
                    else
                    {
                        Close();
                    }
                }
            });
        }

        /// <summary>
        /// 接收
        /// </summary>
        /// <returns></returns>
        public byte[]? Receive()
        {
            if (Connected && clientSocket != null)
            {
                return this.ReceiveProcess(clientSocket, ReceiveBuffer)?.FirstOrDefault();
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// 发送
        /// </summary>
        /// <param name="buffer"></param>
        /// <returns></returns>
        public bool Send(byte[] buffer)
        {
            bool success = false;
            try
            {
                if (Connected && clientSocket != null)
                {
                    int len = clientSocket.Send(buffer);
                    success = buffer.Length == len;
                }
                else
                {
                    Connect();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Connect();
            }
            return success;
        }
        /// <summary>
        /// 释放
        /// </summary>
        public void Dispose()
        {
            Close();
            Timer?.Dispose();
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
            GC.Collect();
        }
    }
}