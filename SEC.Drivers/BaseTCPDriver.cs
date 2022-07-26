using SEC.Util;
using System.Net.Sockets;

namespace SEC.Drivers
{
    public abstract class BaseTCPDriver : IDisposable
    {
        public BaseTCPDriver(string ServerIP, int serverPort)
        {
            ipAddress = ServerIP;
            port = serverPort;
            if (!Connect())
            {
                throw new Exception($"连接:{ IpAddress}:{ Port}超时");
            }
        }
        /// <summary>
        /// 连接套接字
        /// </summary> 
        private readonly UsingLock<Socket> clientSocket = new();

        private readonly string ipAddress;
        /// <summary>
        /// 服务端ip地址
        /// </summary>
        public string IpAddress => ipAddress;
        private readonly int port;
        /// <summary>
        /// 服务端端口
        /// </summary>
        public int Port => port;
        /// <summary>
        /// 连接状态
        /// </summary>
        public bool Connected
        {
            get
            {
                using (clientSocket.Read())
                {
                    return clientSocket.Data?.Connected ?? false;
                }
            }
        }

        /// <summary>
        /// 连接
        /// </summary>
        /// <param name="timeOut"></param>
        /// <returns></returns>
        public bool Connect(int timeOut = 3000)
        {
            try
            {
                using (clientSocket.Write())
                {
                    if (!Connected)
                    {
                        clientSocket.Data = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                        clientSocket.Data.SendTimeout = timeOut;
                        clientSocket.Data.ReceiveTimeout = timeOut;
                        clientSocket.Data.Connect(IpAddress, Port);
                    }
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
            using (clientSocket.Write())
            {
                if (Connected)
                {
                    try
                    {
                        byte[] buffer = new byte[clientSocket.Data.ReceiveBufferSize];
                        int len = clientSocket.Data.Receive(buffer);
                        return buffer.Take(len).ToArray();
                    }
                    catch (Exception)
                    {
                        return null;
                    }
                }
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
            using (clientSocket.Write())
            {
                if (Connected)
                {
                    try
                    {
                        int len = clientSocket.Data.Send(buffer);
                        return buffer.Length == len;
                    }
                    catch (Exception)
                    {
                        return false;
                    }
                }
                return false;
            }
        }
        /// <summary>
        /// 发送并响应
        /// </summary>
        /// <param name="buffer"></param>
        /// <returns></returns>
        public byte[]? SendWait(byte[] buffer)
        {
            for (int i = 0; i < 5; i++)
            {
                if (Send(buffer))
                {
                    Thread.Sleep(10);
                    return Receive();
                }
                else
                {
                    Close();
                    Connect();
                };
                Thread.Sleep(100);
            };
            return null;

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
            using (clientSocket.Write())
            {
                if (Connected)
                    clientSocket.Data?.Disconnect(false);
                clientSocket.Data?.Close();
                clientSocket.Data?.Dispose();
                clientSocket.Data = null;
            }
        }



        /// <summary>
        /// 驱动连接状态
        /// </summary>
        public virtual bool DriverState => Connected;
        /// <summary>
        /// 一个字单位的数据表示的地址长度，西门子为2，三菱，欧姆龙，modbusTcp就为1，AB PLC无效<br />
        /// The address length represented by one word of data, Siemens is 2, Mitsubishi, Omron, modbusTcp is 1, AB PLC is invalid
        /// </summary>
        /// <remarks>
        /// 对设备来说，一个地址的数据对应的字节数，或是1个字节或是2个字节，通常是这两个选择
        /// </remarks>
        public ushort WordLength => _WordLength;
        internal ushort _WordLength = 1;
        /// <summary>
        /// 读取
        /// </summary>
        /// <param name="address"></param>
        /// <param name="length"></param>
        /// <param name="isBit"></param>
        /// <returns></returns>
        public virtual byte[]? Read(string address, ushort length, bool isBit = false)
        {
            throw new Exception("功能未实现");
        }
        /// <summary>
        /// 写入
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="address"></param>
        /// <param name="Value"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public virtual bool Write(string address, byte[] Value, bool isBit)
        {
            throw new Exception("功能未实现");
        }
        /// <summary>
        /// 异步读取
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="address"></param>
        /// <param name="Value"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task<byte[]?> AsyncRead(string address, ushort length, bool isBit)
        {
            return await Task.Run(() => Read(address, length, isBit));
        }
        /// <summary>
        /// 异步写入
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="address"></param>
        /// <param name="Value"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task<bool> AsyncWrite(string address, byte[] Value, bool isBit)
        {
            return await Task.Run(() => Write(address, Value, isBit));
        }
    }
}