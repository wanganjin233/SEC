using SEC.Util;
using System.Text.RegularExpressions;

namespace SEC.Driver.MC3E
{
    public class MC3E : BaseDriver
    {
        public MC3E(string communicationStr)
           : base(communicationStr)
        {
            Communication.HeadBytes = new byte[7] { 0xD0, 0x0, 0x0, 0xFF, 0xFF, 0x03, 0x0 };
            Communication.DataLengthLocation = 7;
            Communication.DataLengthType = LengthTypeEnum.UShort;
        }
        /// <summary>
        /// 生成报文
        /// </summary>
        /// <param name="tagGroup"></param>
        /// <param name="StationNumber"></param>
        /// <param name="TypeEnumtem"></param>
        /// <returns></returns>
        protected override byte[]? BatchReadCommand(TagGroup tagGroup, byte StationNumber, object TypeEnumtem)
        {
            Tag firstTag = tagGroup.Tags.First();
            return firstTag.Location.BatchReadCommand((AddressTypeEnum)TypeEnumtem, tagGroup.Length, firstTag.IsBit, NetworkNumber, StationNumber);
        }
        /// <summary>
        /// 网络号，通常为0
        /// </summary>
        /// <remarks>
        /// 依据PLC的配置而配置，如果PLC配置了1，那么此处也填0，如果PLC配置了2，此处就填2，测试不通的话，继续测试0
        /// </remarks>
        public byte NetworkNumber
        {
            get;
            set;
        } = 0;
        /// <summary>
        /// 读取最大长度
        /// </summary>
        public override int ReadMaxLenth => 960;

        /// <summary>
        /// 发送并接收
        /// </summary>
        /// <param name="command"></param>
        /// <param name="headByte"></param>
        /// <returns></returns>
        public override byte[]? SendCommand(byte[] command)
        {
            int datalen = BitConverter.ToUInt16(new byte[2] { command[19], command[20] }, 0);
            return base.SendCommand(command).GetBody(command[13] == 1, datalen);
        }

        /// <summary>
        /// tag点位地址解析
        /// </summary>
        /// <param name="tag"></param>
        /// <returns></returns>
        protected override Tag TagParsing(Tag tag)
        {
            string? addressType = Regex.Matches(tag.Address, "^[a-zA-Z]+").FirstOrDefault()?.Value;
            if (addressType != null && addressType.ToEnum(out AddressTypeEnum _AddressType))
            {
                tag.Type = _AddressType;
                switch (_AddressType)
                {
                    //位 16 
                    case AddressTypeEnum.X:
                    case AddressTypeEnum.Y:
                    case AddressTypeEnum.B:
                        tag.Location = (uint)tag.Address.Remove(0, addressType.Length).ToInt0X();
                        tag.IsBit = true;
                        break;
                    //位 10
                    case AddressTypeEnum.M:
                    case AddressTypeEnum.L:
                    case AddressTypeEnum.F:
                    case AddressTypeEnum.V:
                    case AddressTypeEnum.S:
                    case AddressTypeEnum.TS:
                    case AddressTypeEnum.TC:
                    case AddressTypeEnum.SS:
                    case AddressTypeEnum.SC:
                    case AddressTypeEnum.CS:
                    case AddressTypeEnum.CC:
                        tag.Location = (uint)tag.Address.Remove(0, addressType.Length).ToInt();
                        tag.IsBit = true;
                        break;
                    //字 16
                    case AddressTypeEnum.W:
                        if (tag.Address.Contains('.'))
                        {
                            var addressSplit = tag.Address.Split('.');
                            tag.Location = (uint)addressSplit[0].Remove(0, addressType.Length).ToInt0X();
                            tag.BitLocation = addressSplit[1].ToInt();
                        }
                        else
                        {
                            tag.Location = (uint)tag.Address.Remove(0, addressType.Length).ToInt0X();
                        }
                        break;
                    case AddressTypeEnum.D:
                    case AddressTypeEnum.TN:
                    case AddressTypeEnum.SN:
                    case AddressTypeEnum.CN:
                        if (tag.Address.Contains('.'))
                        {
                            var addressSplit = tag.Address.Split('.');
                            tag.Location = (uint)addressSplit[0].Remove(0, addressType.Length).ToInt();
                            tag.BitLocation = addressSplit[1].ToInt();
                        }
                        else
                        {
                            tag.Location = (uint)tag.Address.Remove(0, addressType.Length).ToInt();
                        }
                        break;
                }
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
                byte[] command = tag.Location.BatchWriteCommand((AddressTypeEnum)tag.Type, value, tag.IsBit, NetworkNumber, tag.StationNumber);
                return SendCommand(command) != null;
            }
            return false;
        }
    }
}
