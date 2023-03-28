using SEC.Communication;
using SEC.Enum.Driver;
using SEC.Models.Driver;
using SEC.Models.Interactive;
using SEC.Util;
using System.Collections.Concurrent;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;

namespace SEC.Docker.Driver
{
    public class SockerHelper
    {
        /// <summary>
        /// socket客户端
        /// </summary>
        private readonly SocketClient socketClient;
        /// <summary>
        /// 阻塞
        /// </summary>
        private readonly ConcurrentDictionary<string, AutoResetEvent> autoResetEvents = new ConcurrentDictionary<string, AutoResetEvent>();
        /// <summary>
        /// 结果
        /// </summary>
        private readonly ConcurrentDictionary<string, OperateResult<object>> results = new ConcurrentDictionary<string, OperateResult<object>>();

        public bool Connected()
        {
            return socketClient.Connected;
        }
        /// <summary>
        /// 初始化连接
        /// </summary>
        public SockerHelper()
        {
            #region 参数
            string address = "127.0.0.1:30000";
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                address = Environment.GetEnvironmentVariable("address", EnvironmentVariableTarget.Process) ?? address;
            }
            #endregion 
            socketClient = new(address)
            {
                HeadBytes = new byte[2] { 0, 1 },
                EndBytes = new byte[2] { 2, 3 },
                DataLengthLocation = 2,
                DataLengthType = LengthTypeEnum.Uint,
                HeartbeatBytes = new byte[] { 0, 1, 0, 0, 0, 0, 2, 3 },
            };
            socketClient.ReceiveEvent += SocketClient_ReceiveEvent;
            socketClient.Connect();
            CacheSend();
        }
        /// <summary>
        /// 发送
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="operateResult"></param>
        public bool Send<T>(OperateResult<T> operateResult)
        {
            string message = operateResult.ToJson();
            bool success = socketClient.SendConformity(message.ToBytes(Encoding.UTF8));
            if (!success)
            {
                CacheHelper.Write(message);
                CacheReset.Set();
            }
            return success;
        }
        AutoResetEvent CacheReset = new AutoResetEvent(false);
        private void CacheSend()
        {
            Task.Run(() =>
            {
                while (true)
                {
                    if (socketClient.Connected)
                    { 
                        string? message = CacheHelper.Read();
                        if (message != null)
                        {
                            if (socketClient.SendConformity(message.ToBytes(Encoding.UTF8)))
                            {
                                CacheHelper.DeleteFirst();
                            }
                            else
                            {
                                Task.Delay(5000).Wait();
                            }
                        }
                        else
                        {
                            CacheReset.WaitOne();
                        }
                    }
                    else
                    {
                        Task.Delay(5000).Wait(); 
                    }
                }
            });
        }

        /// <summary>
        /// 发送并等待接收
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="operateResult"></param>
        /// <returns></returns>
        public T1? SendWait<T1, T2>(OperateResult<T2> operateResult)
        {
            AutoResetEvent? autoResetEvent = new AutoResetEvent(false);
            T1? result = default;
            if (autoResetEvents.TryAdd(operateResult.MessageCode, autoResetEvent))
            {
                if (socketClient.SendConformity(operateResult.ToJson().ToBytes(Encoding.UTF8)))
                {
                    if (autoResetEvent.WaitOne(5000))
                    {
                        results.TryRemove(operateResult.MessageCode, out OperateResult<object>? value);
                        if (value != null)
                        {
                            result = value.Content.ToJson().ToObject<T1>();
                        }
                    }
                }
                if (autoResetEvents.TryRemove(operateResult.MessageCode, out autoResetEvent))
                    autoResetEvent.Dispose();
            }
            return result;
        }
        /// <summary>
        /// 接收事件
        /// </summary>
        /// <param name="socket"></param>
        /// <param name="bytes"></param>
        private void SocketClient_ReceiveEvent(Socket socket, byte[] bytes)
        {
            var operateResult = bytes.ToString(Encoding.UTF8).ToObject<OperateResult<object>>();
            if (operateResult?.IsSuccess ?? false)
            {
                AutoResetEvent? autoResetEvent = autoResetEvents.GetValueOrDefault(operateResult.MessageCode);
                if (autoResetEvent != null)
                {
                    results.TryAdd(operateResult.MessageCode, operateResult);
                    autoResetEvents.GetValueOrDefault(operateResult.MessageCode)?.Set();
                }
                else if (operateResult.Message == "WriteTag")
                {
                    if (operateResult.Content is Tag tag)
                    {
                        WriteTagEvent?.Invoke(tag);
                    }
                }
            }
        }
        public delegate void WriteTagDelegate(Tag tag);
        public event WriteTagDelegate? WriteTagEvent;


    }
}
