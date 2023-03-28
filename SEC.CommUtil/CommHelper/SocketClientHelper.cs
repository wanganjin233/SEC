using SEC.Communication;
using SEC.Models.Interactive;
using SEC.Util;
using System.Collections.Concurrent;

namespace SEC.CommUtil
{
    partial class ReceiveBlock
    {
        /// <summary>
        /// 阻塞
        /// </summary>
        public AutoResetEvent AutoReset { get; set; } = new AutoResetEvent(false);
        /// <summary>
        /// 接收的结果
        /// </summary>
        public string? Receive { get; set; }
    }
    public class SocketClientHelper
    {
        /// <summary>
        /// Socket客户端
        /// </summary>
        private readonly SocketClient client;
        /// <summary>
        /// 缓存操作
        /// </summary>
        private readonly FileCache cacheHelper;
        /// <summary>
        /// 客户端标识
        /// </summary> 
        public SocketClientInfo ScokerCilentInfo => _cokerCilentInfo;
        private SocketClientInfo _cokerCilentInfo;
        /// <summary>
        /// 接收缓存
        /// </summary>
        private readonly ConcurrentDictionary<string, ReceiveBlock> ReceiveBlocks = new();
        /// <summary>
        /// 接收委托
        /// </summary>
        /// <param name="id">Id</param>
        /// <param name="data">数据</param>
        public delegate void ReceiveDelegate(SocketClientHelper SockerClient, OperateResult<object> data);
        /// <summary>
        /// 接收事件
        /// </summary>
        public event ReceiveDelegate? ReceiveEvent;
        /// <summary>
        /// 基本方法
        /// </summary>
        private readonly ControllerManage BaseMethods;
        /// <summary>
        /// 初始化连接
        /// </summary>
        public SocketClientHelper(string ConnectionString, SocketClientInfo scokerCilentInfo)
        {
            _cokerCilentInfo = scokerCilentInfo;

            client = new SocketClient(ConnectionString)
            {
                HeadBytes = new byte[1] { 02 },
                EndBytes = new byte[1] { 03 },
                LoginBytes = scokerCilentInfo.ToJson().ToBytes(),
                DataLengthType = Enum.Driver.LengthTypeEnum.Uint,
                DataLengthLocation = 1,
                HeartbeatBytes = new byte[1] { 00 },
                SendTimeout = 2000,
                ReceiveTimeout = 5000,
                HeartbeatCycle = 2000,
            };
            client.ReceiveEvent += Client_ReceiveEvent;
            client.ConnectEvent += Client_ConnectEvent;
            client.Connect();
            cacheHelper = new(scokerCilentInfo.ClientId);
            Task.Run(() =>
            {
                while (true)
                {
                    try
                    {
                        string message = cacheHelper.Dequeue();
                        if (client.SendConformity(message.ToBytes()))
                        {
                            cacheHelper.ACK();
                        }
                        else
                        {
                            Task.Delay(5000).Wait();
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }
                }
            });
            BaseMethods = new ControllerManage();
        }
        public ConcurrentBag<OperateResult<object>> ConnectSendDatas = new ConcurrentBag<OperateResult<object>>();
        private void Client_ConnectEvent(System.Net.Sockets.Socket socket)
        {
            foreach (var ConnectSendData in ConnectSendDatas)
            {
                Send(ConnectSendData);
            }
        }

        /// <summary>
        /// 接收数据处理
        /// </summary>
        /// <param name="socket"></param>
        /// <param name="bytes"></param>
        private void Client_ReceiveEvent(System.Net.Sockets.Socket socket, byte[] bytes)
        {
            try
            {
                string data = bytes.ToUTF8String();
                data.TryToObject<OperateResult<object>>(out var operateResult);
                if (operateResult != null)
                {
                    //需要响应的数据
                    if (ReceiveBlocks.TryGetValue(operateResult.MessageCode, out var receiveBlock))
                    {
                        receiveBlock.Receive = data;
                        receiveBlock.AutoReset.Set();
                    }
                    else
                    {
                        //尝试基本方法处理
                        BaseMethods.MethodInfoInvoke(operateResult);
                        //自定义方法处理
                        ReceiveEvent?.Invoke(this, operateResult);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"处理接收信息异常【{e.Message}】");
            }
        }
        /// <summary>
        /// 发送到服务端并等待响应
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="Identity"></param>
        /// <param name="operateResult"></param>
        public OperateResult<T2>? SendWait<T1, T2>(OperateResult<T1> operateResult)
        {
            //添加阻塞
            ReceiveBlock receiveBlock = new();
            ReceiveBlocks.TryAdd(operateResult.MessageCode, receiveBlock);
            //设置发送端标识
            operateResult.SenderIdentity = _cokerCilentInfo.ClientId;
            //发送
            var dataBytes = operateResult.ToJson().ToBytes();
            client.SendConformity(dataBytes);
            //等待结果
            string? receive = null;
            if (receiveBlock.AutoReset.WaitOne(5000)
                && receiveBlock.Receive != null)
            {
                receive = receiveBlock.Receive;
            }
            //移除阻塞
            ReceiveBlocks.TryRemove(operateResult.MessageCode, out _);
            if (receive?.TryToObject(out OperateResult<T2>? result) ?? false)
            {
                return result;
            }
            return null;
        }

        /// <summary>
        /// 发送到服务端
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="Identity"></param>
        /// <param name="operateResult"></param>
        public void Send<T>(OperateResult<T> operateResult)
        {
            var msg = operateResult.ToJson();
            var dataBytes = msg.ToBytes();
            if (!client.SendConformity(dataBytes))
            {
                cacheHelper.Enqueue(msg);
            }
        }
        /// <summary>
        /// 回复信息
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="operateResult"></param>
        public void Reply<T>(OperateResult<T> operateResult)
        {
            operateResult.ReceiverIdentity = operateResult.SenderIdentity;
            operateResult.SenderIdentity = null;
            Send(operateResult);
        }
    }
}
