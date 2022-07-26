using SEC.DataFormatModel.TagInfo;
using SEC.Drivers.OmronFinsEthernetDriver;
using System.Text.RegularExpressions;

namespace SEC.Drivers
{
    public class OmronFinsEthernet : BaseTCPDriver, IBaseDriver
    {
        /// <summary>
        /// PLC节点
        /// </summary>
        private byte? PLCNode = null;
        /// <summary>
        /// PC节点
        /// </summary>
        private byte? PCNode = null;
        /// <summary>
        /// 驱动状态
        /// </summary>
        public override bool DriverState => Connected && PLCNode != null && PCNode != null;
        /// <summary>
        /// 初始化欧姆龙Fins驱动
        /// </summary>
        /// <param name="ServerIP"></param>
        /// <param name="serverPort"></param>
        /// <param name="isPersistentConn"></param>
        public OmronFinsEthernet(string ServerIP, int serverPort) :
           base(ServerIP, serverPort)
        {
            byte[]? LoginResult = SendWait(OmronFinsEthernetCommand.LogInCommand).GetLogIn();
            if (LoginResult != null)
            {
                PLCNode = LoginResult[7];
                PCNode = LoginResult[3];
            }
        }
        /// <summary>
        /// 批量读取
        /// </summary>
        /// <param name="address"></param>
        /// <param name="length"></param>
        /// <param name="isBit"></param>
        /// <returns></returns>
        public override byte[]? Read(string address, ushort length, bool isBit = false)
        {
            if (DriverState)
            {
                string? addressType = Regex.Matches(address, "^[a-zA-Z]+").FirstOrDefault()?.Value;
                string[] addressSplit = address.Split('.');
                byte bitAddress = 0;
                if (addressType != null
                    && ushort.TryParse(addressSplit[0].Remove(0, addressType.Length), out ushort _address)
                    && addressType.ToEnumByte(out AddressTypeEnum _AddressType)
                    && (addressSplit.Length == 2 ? byte.TryParse(addressSplit[1], out bitAddress) : true))
                {
                    byte[] command = _address.BatchReadCommand(_AddressType, length, isBit, PLCNode, PCNode, bitAddress);
                    return SendWait(command).GetBody();
                }
            }
            return null;
        }
        /// <summary>
        /// 批量写入
        /// </summary>
        /// <param name="address"></param>
        /// <param name="Value"></param>
        /// <param name="isBit"></param>
        /// <returns></returns>
        public override bool Write(string address, byte[] Value, bool isBit = false)
        {
            string? addressType = Regex.Matches(address, "^[a-zA-Z]+").FirstOrDefault()?.Value;
            string[] addressSplit = address.Split('.');
            byte bitAddress = 0;
            if (addressType != null
                && ushort.TryParse(addressSplit[0].Remove(0, addressType.Length), out ushort _address)
                && addressType.ToEnumByte(out AddressTypeEnum _AddressType)
                && (addressSplit.Length == 2 ? byte.TryParse(addressSplit[1], out bitAddress) : true))
            {
                byte[] command = _address.BatchWriteCommand(_AddressType, Value, isBit, PLCNode, PCNode, bitAddress);
                return SendWait(command).GetBody() != null;
            }
            return false;
        }

        public Dictionary<string, List<Tag>> AddressTransition(List<Tag> tagValues)
        {
            throw new NotImplementedException();
        }
    }
}
