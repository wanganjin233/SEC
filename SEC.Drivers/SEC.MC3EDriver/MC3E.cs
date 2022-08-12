using SEC.Driver;
using System.Text.RegularExpressions;

namespace SEC.Driver.MC3E
{
    public class MC3E : BaseDriver
    {
        public MC3E(ICommunication communication) : base(communication)
        {
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
        /// 网络站号，通常为0
        /// </summary>
        /// <remarks>
        /// 依据PLC的配置而配置，如果PLC配置了1，那么此处也填0，如果PLC配置了2，此处就填2，测试不通的话，继续测试0
        /// </remarks>
        public byte NetworkStationNumber
        {
            get;
            set;
        } = 0;


        /// <summary>
        /// 批量读取原始数据
        /// </summary>
        /// <param name="address"></param>
        /// <param name="length"></param>
        /// <param name="isBit"></param>
        /// <returns></returns>
        public override byte[]? Read(string address, ushort length, bool isBit = false)
        {
            string? addressType = Regex.Matches(address, "^[a-zA-Z]+").FirstOrDefault()?.Value;
            if (addressType != null
                && int.TryParse(address.Remove(0, addressType.Length), out int _address)
                && addressType.ToEnumByte(out AddressTypeEnum _AddressType))
            {
                byte[] command = _address.BatchReadCommand(_AddressType, length, isBit);
                return SendWait(command).GetBody(isBit, length);
            }
            return null;
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
            string? addressType = Regex.Matches(address, "^[a-zA-Z]+").FirstOrDefault()?.Value;
            if (addressType != null
                && int.TryParse(address.Remove(0, addressType.Length), out int _address)
                && addressType.ToEnumByte(out AddressTypeEnum _AddressType))
            {
                byte[] command = _address.BatchWriteCommand(_AddressType, Value, isBit);
                return SendWait(command).GetBody(false, Value.Length) != null;
            }
            return false;
        }
    }
}
