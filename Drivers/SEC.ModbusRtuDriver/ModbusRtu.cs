using SEC.Driver;
using SEC.Util;

namespace SEC.ModbusRtuDriver
{
    public class ModbusRtu : BaseDriver
    {
        public ModbusRtu(ICommunication communication)
           : base(communication)
        {

        }
        public int WriteMaxLenth = 123;
        public int ReadMaxLenth = 124;
        /// <summary>
        /// 站号
        /// </summary>
        public byte StationNumber
        {
            get;
            set;
        } = 1;
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
                byte[] command = _address.BatchReadCommand(length, isBit, StationNumber);
                return SendWait(command).GetBody(isBit, length);
            }
            return null;
        }
        /// <summary>
        /// 发送命令并返回
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        public override byte[]? SendCommand(byte[] command)
        {
            return SendWait(command).GetBody();
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
            //排序
            tags = tags.OrderBy(p => p.Address.ToInt()).ToList();
            //获取结束位置
            Func<Tag, int> GetEndPosition = (tag) => tag.Address.ToInt() + ReadMaxLenth;
            int endTag = GetEndPosition(tags.First());
            //生成组地址报文
            Action<TagGroup> CreationReadCommand = (tagGroup) =>
            {
                Tag firstTag = tagGroup.Tags.First();
                Tag lastTag = tagGroup.Tags.Last();
                ushort length = (ushort)(lastTag.Address.ToUshort() + lastTag.DataLength - firstTag.Address.ToUshort());
                tagGroup.Command = firstTag.Address.ToUshort().BatchReadCommand(length, false, StationNumber);
                tagGroup.StartAddress = firstTag.Address.ToUshort();
            };

            TagGroup tagGroup = new TagGroup();
            foreach (var tag in tags)
            {
                if (tag.Address.ToInt() + tag.DataLength < endTag)
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
            return TagGroups.ToList();
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
                byte[] command = _address.BatchWriteCommand(Value, isBit, StationNumber);
                return SendWait(command).WriteVerify(command);
            }
            return false;
        }
    }
}