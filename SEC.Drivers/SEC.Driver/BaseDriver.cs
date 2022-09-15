using SEC.Util;
using System.Collections.Concurrent;
using System.ComponentModel.DataAnnotations;

namespace SEC.Driver
{
    public abstract class BaseDriver : IDisposable
    {
        public BaseDriver(ICommunication communication)
        {
            using (Communication.Write())
            {
                Communication.Data = communication;
            }
        }
        /// <summary>
        /// 连接方式
        /// </summary>
        private UsingLock<ICommunication> Communication = new UsingLock<ICommunication>();
        /// <summary>
        /// 点位组
        /// </summary>
        protected ConcurrentBag<TagGroup> TagGroups = new();
        /// <summary>
        /// 全部点位
        /// </summary>
        protected ConcurrentDictionary<string, Tag> AllTagDic = new();
        /// <summary>
        /// 全部点位
        /// </summary>
        public List<Tag> AllTags => AllTagDic.Values.ToList();
        /// <summary>
        /// 驱动连接状态
        /// </summary>
        public virtual bool DriverState => Communication.Data?.Connected ?? false;
        /// <summary>
        /// 一个字的数据地址长度 
        /// 西门子 2
        /// 三菱，欧姆龙，modbusTcp 1
        /// </summary> 
        public virtual ushort WordLength => 1;
        /// <summary>
        /// 发送并响应
        /// </summary>
        /// <param name="buffer"></param>
        /// <returns></returns>
        public virtual byte[]? SendWait(byte[] buffer)
        {
            return null;
        }
        protected virtual int GetLength(byte[] bytes, int startIndex)
        {
            throw new Exception("功能未实现");
        }
        protected byte[]? SendResponse(byte[] command, byte[] headByte)
        {
            using (Communication.Write())
            {
                if (Communication.Data?.Send(command) ?? false)
                {
                    Thread.Sleep(10);
                    byte[] data = new byte[8192];
                    int index = 0;
                    int headIndex = -1;
                    int len = -1;
                    do
                    {
                        byte[]? bytes = Communication.Data.Receive();
                        if (bytes != null)
                        {
                            bytes.CopyTo(data, index);
                            if (headIndex == -1)
                            {
                                headIndex = data.IndexOf(headByte);
                                if (headIndex > -1)
                                {
                                    len = GetLength(bytes, headIndex);
                                }
                            }
                            index = bytes.Length;
                        }
                        else
                        {
                            return null;
                        }
                    } while (index < headByte.Length || (headIndex > -1 && len != index));
                    return data.Skip(headIndex).Take(len - headIndex).ToArray();
                }
                else
                {
                    Communication.Data?.Connect();
                }
                return null;
            }
        }
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
        public virtual bool Write(Tag tag, byte[] value)
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
        /// 发送命令并返回
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        public virtual byte[]? SendCommand(byte[] command)
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
        protected virtual List<TagGroup>? Packet(List<Tag> tags)
        {
            throw new Exception("未实现分组功能");
        }
        /// <summary>
        /// 释放
        /// </summary>
        public void Dispose()
        {
            Stop();
            Communication?.Dispose();
            GC.SuppressFinalize(this);
        }
        protected virtual void AddressParsing(Tag tag)
        {
            throw new Exception("未实现分组功能");
        }
        /// <summary>
        /// 添加节点
        /// </summary>
        /// <param name="tags"></param>
        public void AddTags(List<Tag> tags)
        {
            tags.ForEach(p =>
            {
                AddressParsing(p);
                AllTagDic.TryAdd(p.Address, p);
                p.baseDriver = this;
            });
            Packet(AllTagDic.Values.ToList());
        }
        /// <summary>
        /// 删除节点
        /// </summary>
        /// <param name="tags"></param>
        public void RemoveTags(List<Tag> tags)
        {
            tags.ForEach(p =>
            {
                AllTagDic.Remove(p.Address, out Tag? tag);
            });
            Packet(AllTagDic.Values.ToList());
        }
        /// <summary>
        /// 启动驱动
        /// </summary>
        /// <param name="cycle"></param>
        public void Start(int cycle = 100)
        {
            if (IsRun == false)
            {
                IsRun = true;
                using (Communication.Write())
                {
                    Communication.Data?.Connect();
                }
                AllTagDic.Values.ToList().ForEach(tag => tag.Cycle = cycle);
                Task.Run(async () =>
               {
                   while (IsRun)
                   {
                       foreach (var tagGroup in TagGroups)
                       {
                           if (tagGroup.Command == null) continue;
                           byte[]? BodyByte = SendCommand(tagGroup.Command);
                           tagGroup.Tags.ForEach(p =>
                           {
                               p.UpdateValue = BodyByte?
                               .Skip((int)((p.Location - tagGroup.StartAddress) * (p.DataType == TagTypeEnum.Boole ? 1 : 2)))
                               .Take(p.DataLength)
                               .ToArray();
                           });
                       }
                       await Task.Delay(cycle);
                   }
               });
            }
        }
        /// <summary>
        /// 运行状态
        /// </summary>
        private bool IsRun = false;
        /// <summary>
        /// 停止驱动
        /// </summary>
        public void Stop()
        {
            IsRun = false;
        }
    }
}
