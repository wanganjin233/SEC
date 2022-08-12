using SEC.Driver;
using SEC.Util;

namespace SEC.Driver.ModebusTcp
{
    public class ModbusTcp : BaseDriver
    {
        public ModbusTcp(ICommunication communication)
           : base(communication)
        { }
        public int WriteMaxLenth = 120;
        public int ReadMaxLenth = 120;
        /// <summary>
        /// 消息标识
        /// </summary>
        private ushort identifying = 0;
        /// <summary>
        /// 设置标识
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        private byte[] SetIdentifying(byte[] command)
        {
            byte[] headByte = new byte[4];
            byte[] _Identifying = BitConverter.GetBytes(++identifying);
            _Identifying.CopyTo(headByte, 0);
            headByte.CopyTo(command, 0);
            return headByte;
        }
        /// <summary>
        /// 批量读取原始数据
        /// </summary>
        /// <param name="address"></param>
        /// <param name="length"></param>
        /// <param name="isBit"></param>
        /// <returns></returns>
        public override byte[]? Read(string address, ushort length, bool isBit = false)
        {
            if (ushort.TryParse(address, out ushort _address))
            {
                byte[] command = _address.BatchReadCommand(length, (byte)(isBit ? 0x01 : 0x03), 1);
                byte[] headByte = SetIdentifying(command);
                return SendResponse(command, headByte).GetBody(isBit, length);
            }
            return null;
        }
        /// <summary>
        /// 地址处理
        /// </summary>
        /// <param name="tag"></param>
        /// <exception cref="Exception">未找到地址类型</exception>
        protected override void AddressParsing(Tag tag)
        {
            char addressType = tag.Address[0];
            tag.Type = addressType switch
            {
                '0' => 0x01,
                '1' => 0x02,
                '3' => 0x04,
                '4' => 0x03,
                _ => throw new Exception(),
            };
            if (addressType == '1' || addressType == '3')
            {
                tag.ClientAccess = "R";
            }
            tag.Location = tag.Address[1..].ToUshort();
        }

        public override bool Write(Tag tag, byte[] value)
        {
            if (tag.ClientAccess != "R")
            {
                byte[] command = ((ushort)tag.Location).BatchWriteCommand(value, tag.Type == 0x01, 1);
                byte[] headByte = SetIdentifying(command);
                return SendResponse(command, headByte).Verify();
            }
            return false;
        }

        /// <summary>
        /// 批量写入原始数据
        /// </summary>
        /// <param name="address"></param>
        /// <param name="length"></param>
        /// <param name="isBit"></param>
        /// <returns></returns>
        public override bool Write(string address, byte[] Value, bool isBit = false)
        {
            if (ushort.TryParse(address, out ushort _address))
            {
                byte[] command = _address.BatchWriteCommand(Value, isBit, 1);
                byte[] headByte = SetIdentifying(command);
                return SendResponse(command, headByte).Verify();
            }
            return false;
        }
        /// <summary>
        /// 发送命令
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        public override byte[]? SendCommand(byte[] command)
        {
            byte[] headByte = SetIdentifying(command);
            return SendResponse(command, headByte).GetBody(command[7] == 2 || command[7] == 1, BitConverter.ToUInt16(command.Reverse().ToArray()));
        }

        /// <summary>
        /// 获取包长度
        /// </summary>
        /// <param name="bytes"></param>
        /// <param name="startIndex"></param>
        /// <returns></returns>
        protected override int GetLength(byte[] bytes, int startIndex)
        {
            var _data = bytes.Skip(startIndex + 4).Take(2).Reverse();
            return BitConverter.ToUInt16(_data.ToArray()) + 6 + startIndex;
        }
        /// <summary>
        /// 分组
        /// </summary>
        /// <param name="tags"></param> 
        protected override List<TagGroup>? Packet(List<Tag> tags)
        {
            //清空分组
            TagGroups.Clear();
            if (!tags.Any()) { return null; }
            foreach (var tagGByStationNumber in tags.GroupBy(p => p.StationNumber))
            {
                foreach (var tagGByTypeEnumtem in tagGByStationNumber.GroupBy(p => p.Type))
                {
                    //排序
                    List<Tag> tagsList = tagGByTypeEnumtem.OrderBy(p => p.Location).ToList();
                    //获取结束位置
                    Func<Tag, int> GetEndPosition = (tag) => (int)(tag.Location + ReadMaxLenth);
                    int endTag = GetEndPosition(tagsList.First());
                    //生成组地址报文
                    Action<TagGroup> CreationReadCommand = (tagGroup) =>
                    {
                        Tag firstTag = tagGroup.Tags.First();
                        Tag lastTag = tagGroup.Tags.Last();
                        tagGroup.Length = (ushort)(lastTag.Location + lastTag.DataLength / 2 - firstTag.Location);
                        tagGroup.Command = ((ushort)firstTag.Location).BatchReadCommand(tagGroup.Length, tagGByTypeEnumtem.Key, tagGByStationNumber.Key);
                        tagGroup.StartAddress = (ushort)firstTag.Location;
                    };
                    TagGroup tagGroup = new TagGroup();
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
            return TagGroups.ToList();
        }
    }
}