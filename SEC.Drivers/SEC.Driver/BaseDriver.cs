using SEC.Util;
using System.Collections.Concurrent;

namespace SEC.Driver
{
    public abstract class BaseDriver : IDisposable
    {
        public BaseDriver(string communicationStr)
        {
            Communication = Tools.ConnectionResolution(communicationStr);
            Communication.ReceiveTimeout = 5000;
            Communication.SendTimeout = 5000;
            Communication.Connect();
        }
        private readonly object _lock = new();
        /// <summary>
        /// 连接需求
        /// </summary>
        public readonly ICommunication Communication;
        /// <summary>
        ///最大 读取长度
        /// </summary>
        public virtual int ReadMaxLenth => 124;
        /// <summary>
        /// 驱动连接状态
        /// </summary>
        public virtual bool DriverState => Communication.Connected;
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
        /// <summary>
        /// 写入
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="address"></param>
        /// <param name="Value"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception> 
        public virtual bool Write(Tag tag, byte[] value)
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
            lock (_lock)
            {
                if (Communication.Send(command))
                {
                    Thread.Sleep(10);
                    return Communication.Receive();
                }
                return null;
            }
        }
        /// <summary>
        /// 生成报文
        /// </summary>
        /// <param name="tagGroup"></param>
        /// <param name="StationNumber"></param>
        /// <param name="TypeEnumtem"></param>
        /// <returns></returns>
        protected virtual byte[]? BatchReadCommand(TagGroup tagGroup, byte StationNumber, object TypeEnumtem)
        {
            throw new Exception("未实现");
        }

        /// <summary>
        /// tag分组
        /// </summary>
        /// <param name="tags"></param>
        /// <returns></returns>
        protected virtual List<TagGroup>? Packet(List<Tag> tags)
        {
            //清空分组
            TagGroups.Clear();
            if (!tags.Any()) { return null; }
            foreach (var tagGByStationNumber in tags.GroupBy(p => p.StationNumber))
            {
                foreach (var tagGByBit in tagGByStationNumber.GroupBy(p => p.IsBit))
                {
                    foreach (var tagGByTypeEnumtem in tagGByBit.GroupBy(p => p.Type))
                    {
                        TagGroup tagGroup = new()
                        {
                            IsBit = tagGByBit.Key
                        };
                        //排序
                        List<Tag> tagsList = tagGByTypeEnumtem.OrderBy(p => p.Location).ToList();
                        //生成组地址报文
                        void CreationReadCommand(TagGroup tagGroup)
                        {
                            Tag firstTag = tagGroup.Tags.First();
                            Tag lastTag = tagGroup.Tags.Last();
                            tagGroup.Length = (ushort)(lastTag.Location + Math.Ceiling(lastTag.DataLength / 2.0) - firstTag.Location);
                            tagGroup.Command = BatchReadCommand(tagGroup, tagGByStationNumber.Key, tagGByTypeEnumtem.Key);
                            tagGroup.StartAddress = (ushort)firstTag.Location;
                        }
                        //获取结束位置 
                        int GetEndPosition(Tag tag) => (int)(tag.Location + ReadMaxLenth);
                        int endTag = GetEndPosition(tagsList.First());
                        foreach (var tag in tagsList)
                        {
                            if (tag.Location + tag.DataLength / 2 < endTag)
                            {
                                tagGroup.Tags.Add(tag);
                            }
                            else
                            {
                                TagGroups.Add(tagGroup);
                                CreationReadCommand(tagGroup);
                                tagGroup = new TagGroup();
                                tagGroup.Tags.Add(tag);
                                endTag = GetEndPosition(tag);
                            }
                        }
                        TagGroups.Add(tagGroup);
                        CreationReadCommand(tagGroup);
                    }
                }
            }
            return TagGroups.ToList();
        }
        /// <summary>
        /// 释放
        /// </summary>
        public void Dispose()
        {
            Stop();
            GC.SuppressFinalize(this);
        }
        /// <summary>
        /// tag处理
        /// </summary>
        /// <param name="tag"></param>
        /// <returns></returns>
        protected virtual Tag TagParsing(Tag tag)
        {
            return tag;
        }
        /// <summary>
        /// 添加节点
        /// </summary>
        /// <param name="tags"></param>
        public virtual void AddTags(List<Tag> tags)
        {
            tags.ForEach(p =>
            {
                p = TagParsing(p);
                AllTagDic.TryAdd(p.TagName, p);
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
                AllTagDic.Remove(p.TagName, out Tag? tag);
            });
            Packet(AllTagDic.Values.ToList());
        }
        /// <summary>
        /// 启动驱动
        /// </summary>
        /// <param name="cycle"></param>
        public virtual void Start(int cycle = 100)
        {
            if (IsRun == false)
            {
                IsRun = true;
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
                               int skipIndex;
                               if (tagGroup.IsBit)
                               {
                                   if (p.BitLocation != -1)
                                   {
                                       skipIndex = (int)(p.Location - tagGroup.StartAddress) * 16 + p.BitLocation;
                                   }
                                   else
                                   {
                                       skipIndex = (int)(p.Location - tagGroup.StartAddress);
                                   }
                               }
                               else
                               {
                                   skipIndex = (int)(p.Location - tagGroup.StartAddress) * 2;
                               }
                               p.UpdateValue = BodyByte?
                               .Skip(skipIndex)
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
            Communication.Dispose();
            IsRun = false;
        }
    }
}
