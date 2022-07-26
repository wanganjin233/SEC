using SEC.DataFormatModel.TagInfo;
using SEC.Drivers.ModbusRtuSerialPortDriver;

namespace SEC.Drivers 
{
    public class ModbusRtuSerialPort : BaseSerialPortDriver, IBaseDriver
    {
        public ModbusRtuSerialPort(string portName, int baudRate, int dataBits, string parity, string stopBits) :
          base(portName, baudRate, dataBits, parity, stopBits)
        {
        }
        /// <summary>
        /// 站号
        /// </summary>
        public byte StationNumber
        {
            get;
            set;
        } = 1;

        public Dictionary<string, List<Tag>> AddressTransition(List<Tag> tagValues)
        {
            
            throw new NotImplementedException();
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
                byte[] command = _address.BatchReadCommand(length, isBit, StationNumber);
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
            if (ushort.TryParse(address, out ushort _address))
            {
                byte[] command = _address.BatchWriteCommand(Value, isBit, StationNumber); 
                return SendWait(command).WriteVerify(command);
            }
            return false;
        }
    }
}