using SEC.Util;
namespace SEC.Driver.ModebusTcp
{
    public class ModbusTcp : BaseDriver
    {
        #region 初始化  
        public ModbusTcp(string communicationStr)
           : base(communicationStr)
        {
            Communication.DataLengthLocation = 4;
            Communication.DataLengthType = LengthTypeEnum.ReUShort;
            BatchReadCommand = (tagGroup, StationNumber, TypeEnumtem) =>
            {
                Tag firstTag = tagGroup.Tags.First();
                return ((ushort)firstTag.Location).BatchReadCommand(tagGroup.Length, (byte)(AddressTypeEnum)TypeEnumtem, StationNumber);
            };
            GetEndPosition = (tag) => (int)(tag.Location + ReadMaxLenth);
        }

        #endregion
        #region 属性  
        /// <summary>
        /// 最大读取长度
        /// </summary> 
        public int ReadMaxLenth = 120;
        /// <summary>
        /// 消息标识
        /// </summary>
        private ushort identifying = 0;
        #endregion
        #region 驱动私有方法
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
        #endregion
        #region 重写方法 
        /// <summary>
        /// tag点位地址解析
        /// </summary>
        /// <param name="tag"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        protected override Tag TagParsing(Tag tag)
        {
            switch (tag.Address[0])
            {
                case '0':
                    tag.Type = AddressTypeEnum.zero;
                    tag.IsBit = true;
                    break;
                case '1':
                    tag.Type = AddressTypeEnum.one;
                    tag.ClientAccess = "R";
                    tag.IsBit = true;
                    break;
                case '3':
                    tag.Type = AddressTypeEnum.threea;
                    tag.ClientAccess = "R";
                    break;
                case '4':
                    tag.Type = AddressTypeEnum.four;
                    break;
                default:
                    throw new Exception("地址错误");
            }
            if (tag.Address.Contains('.'))
            {
                tag.DataType = TagTypeEnum.Boole;
                var addressSplit = tag.Address.Split('.');
                addressSplit[1].ToInt();
                tag.Location = addressSplit[0][1..].ToUshort();
            }
            else
            {
                tag.Location = tag.Address[1..].ToUshort();
            }
            return tag;
        }
        /// <summary>
        ///  写入数据
        /// </summary>
        /// <param name="address"></param>
        /// <param name="length"></param>
        /// <param name="isBit"></param>
        /// <returns></returns> 
        public override bool Write(Tag tag, byte[] value)
        {
            if (tag.ClientAccess.Contains('W'))
            {
                byte[] command = ((ushort)tag.Location).BatchWriteCommand(value, tag.IsBit, tag.StationNumber);
                return SendCommand(command) != null;
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
            Communication.HeadBytes = SetIdentifying(command);
            var ada = base.SendCommand(command).GetBody(command[7] == 2 || command[7] == 1, BitConverter.ToUInt16(command.Reverse().ToArray())); ;
            return ada;
        }
        #endregion
    }
}