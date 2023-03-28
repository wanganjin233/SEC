using SEC.Communication;
using SEC.Models.Interactive;
using SEC.Util;
using System.Collections.Concurrent;
using System.Net.Sockets;

namespace SEC.CommUtil
{
    public class SocketServerHelper
    {
        /// <summary>
        /// socket服务端
        /// </summary>
        private readonly SocketServer server;
        /// <summary>
        /// 缓存池
        /// </summary>
        private readonly ConcurrentDictionary<string, FileCache> cacheHelpers = new();

        /// <summary>
        /// 客户端连接池
        /// Key=客户端信息string,Value=客户端信息
        /// </summary>
        public readonly ConcurrentDictionary<string, SocketClientInfo> ScokerCilentInfos = new();
        /// <summary>
        /// 接收委托
        /// </summary>
        /// <param name="id">Id</param>
        /// <param name="data">数据</param>
        public delegate void ReceiveDelegate(string id, SocketClientInfo socketClient, OperateResult<object> data);
        /// <summary>
        /// 接收事件
        /// </summary>
        public event ReceiveDelegate? ReceiveEvent;
        /// <summary>
        /// 接收缓存
        /// </summary>
        private readonly ConcurrentDictionary<string, ReceiveBlock> ReceiveBlocks = new();
        /// <summary>
        /// 服务名称
        /// </summary>
        public string ServerName { get; }
        /// <summary>
        /// 初始化连接
        /// </summary>
        public SocketServerHelper(int port, string serverName)
        {
            ServerName = serverName;
            server = new SocketServer(port, 500)
            {
                HeadBytes = new byte[1] { 02 },
                EndBytes = new byte[1] { 03 },
                LoginAsk = true,
                DataLengthType = Enum.Driver.LengthTypeEnum.Uint,
                HeartbeatBytes = new byte[1] { 00 },
                DataLengthLocation = 1,
                SendTimeout = 2000,
                ReceiveTimeout = 5000
            };
            server.ConnectEvent += Server_ConnectEvent;
            server.ReceiveEvent += Server_ReceiveEvent;
            server.DisconnectEvent += Server_DisconnectEvent;
            server.Start();
        }
        /// <summary>
        /// 接收
        /// </summary>
        /// <param name="ClientId"></param>
        /// <param name="bytes"></param>
        private void Server_ReceiveEvent(string ClientId, byte[] bytes)
        {
            string data = bytes.ToUTF8String();
            data.TryToObject<OperateResult<object>>(out var operateResult);
            if (operateResult != null)
            {
                if (ReceiveBlocks.TryGetValue(operateResult.MessageCode, out var receiveBlock))
                {
                    receiveBlock.Receive = data;
                    receiveBlock.AutoReset.Set();
                }
                else if (ScokerCilentInfos.TryGetValue(ClientId, out SocketClientInfo? socketClient))
                {
                    ReceiveEvent?.Invoke(ClientId, socketClient, operateResult);
                }
            }
        }

        /// <summary>
        /// 连接断开回调
        /// </summary>
        /// <param name="ClientId"></param>
        /// <param name="socket"></param>
        private void Server_DisconnectEvent(string ClientId, Socket socket)
        {
            ScokerCilentInfos.Remove(ClientId, out _);
        }

        /// <summary>
        /// 连接事件
        /// </summary>
        /// <param name="ClientId"></param>
        /// <param name="socket"></param>
        private void Server_ConnectEvent(string loginData, Socket socket)
        {
            loginData.TryToObject(out SocketClientInfo? socketClientInfo);
            if (socketClientInfo != null)
            {
                Console.WriteLine($"接收到登录信息【{loginData}】");
                socketClientInfo.Socket = socket;
                ScokerCilentInfos.TryAdd(loginData, socketClientInfo);
                cacheHelpers.GetOrAdd(socketClientInfo.ClientId, AddFileCache(loginData, socketClientInfo));
            }
            else
            {
                server.RemoveClient(loginData);
            }
        }
        /// <summary>
        /// 添加缓存方法
        /// </summary>
        /// <param name="loginData"></param>
        /// <param name="socketClientInfo"></param>
        /// <returns></returns>
        public FileCache AddFileCache(string loginData, SocketClientInfo socketClientInfo)
        {
            FileCache cacheHelper = new(Path.Combine(ServerName, socketClientInfo.ClientId));
            Task.Run(() =>
            {
                while (true)
                {
                    string message = cacheHelper.Dequeue();
                    if (server.SendConformity(loginData, message.ToBytes()))
                    {
                        cacheHelper.ACK();
                    }
                    else
                    {
                        Task.Delay(5000).Wait();
                    }
                }
            });
            return cacheHelper;
        }
        /// <summary>
        /// 发送到客户端并等待响应
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="Identity"></param>
        /// <param name="operateResult"></param>
        public OperateResult<T2>? SendWait<T1, T2>(string id, OperateResult<T1> operateResult)
        {
            ReceiveBlock receiveBlock = new ReceiveBlock();
            ReceiveBlocks.TryAdd(operateResult.MessageCode, receiveBlock);
            var msg = operateResult.ToJson();
            var dataBytes = msg.ToBytes();
            server.SendConformity(id, dataBytes);
            if (receiveBlock.AutoReset.WaitOne(5000) && receiveBlock.Receive != null)
            {
                receiveBlock.Receive.TryToObject(out OperateResult<T2>? result);
                return result;
            }
            else
            {
                return null;
            }
        }
        /// <summary>
        /// 发送到指定客户端
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="id"></param>
        /// <param name="Identifier"></param>
        /// <param name="operateResult"></param>
        /// <param name="Cache"></param>
        public void Send<T>(string id, OperateResult<T> operateResult, bool Cache = true)
        {
            ScokerCilentInfos.TryGetValue(id, out SocketClientInfo? socketClientInfo);
            if (socketClientInfo != null)
            {
                FileCache cacheHelper = cacheHelpers.GetOrAdd(socketClientInfo.ClientId, AddFileCache(id, socketClientInfo));
                var msg = operateResult.ToJson();
                var dataBytes = msg.ToBytes();

                if (!server.SendConformity(id, dataBytes))
                {
                    if (Cache && msg != null)
                    {
                        cacheHelper.Enqueue(msg);
                    }
                };
            }
        }
        /// <summary>
        /// 回复
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="id"></param>
        /// <param name="operateResult"></param>
        public void Reply<T>(string id, OperateResult<T> operateResult)
        {
            operateResult.ReceiverIdentity = operateResult.SenderIdentity;
            operateResult.SenderIdentity = null;
            Send(id, operateResult, false);
        }
    }
}
