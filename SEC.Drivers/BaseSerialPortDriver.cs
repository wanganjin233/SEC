using SEC.Util;
using System.IO.Ports;

namespace SEC.Drivers
{
    public abstract class BaseSerialPortDriver :  IDisposable
    {
        public BaseSerialPortDriver(string portName, int baudRate, int dataBits, string parity, string stopBits)
        {
            _PortName = portName;
            _BaudRate = baudRate;
            _DataBits = dataBits;
            _Parity = parity switch
            {
                "奇数" => _Parity = Parity.Odd,
                "偶数" => _Parity = Parity.Even,
                "标记" => _Parity = Parity.Mark,
                "空格" => _Parity = Parity.Space,
                _ => _Parity = Parity.None
            };
            _StopBits = stopBits switch
            {
                "1" => StopBits.One,
                "1.5" => StopBits.OnePointFive,
                "2" => StopBits.Two,
                _ => StopBits.None
            };

            if (!Connect())
            {
                throw new Exception($"连接:{ portName}失败");
            }
        }
        /// <summary>
        /// 串口对象
        /// </summary>  
        /// 
        private UsingLock<SerialPort> serialPort = new();
        /// <summary>
        /// 连接状态
        /// </summary>
        public bool Connected
        {
            get
            {
                using (serialPort.Read())
                {
                    return serialPort.Data?.IsOpen ?? false;
                }
            }
        }
        /// <summary>
        /// 串口名
        /// </summary>
        private readonly string _PortName = string.Empty;
        /// <summary>
        /// 波特率
        /// </summary>
        private readonly int _BaudRate = 9600;
        /// <summary>
        /// 数据位
        /// </summary>
        private readonly int _DataBits = 8;
        /// <summary>
        /// 奇偶数
        /// </summary>
        private readonly Parity _Parity = Parity.None;
        /// <summary>
        /// 停止位
        /// </summary>
        private readonly StopBits _StopBits = StopBits.One;
        /// <summary>
        /// 公开串口名
        /// </summary>
        public string PortName => _PortName;
        /// <summary>
        /// 初始化串口设定
        /// </summary>
        /// <param name="portName">串口名称</param>
        /// <param name="baudRate">波特率</param>
        /// <param name="dataBits">数据位</param>
        /// <param name="parity">奇偶数</param>
        /// <param name="stopBits">停止位</param>
        /// <param name="timeout">响应超时</param> 
        /// <summary>
        /// 连接
        /// </summary>
        /// <param name="timeOut">响应超时</param>
        /// <returns></returns>
        public bool Connect(int timeOut = 3000)
        {
            try
            {
                using (serialPort.Write())
                {
                    if (!Connected)
                    {
                        serialPort.Data = new SerialPort(_PortName, _BaudRate, _Parity, _DataBits, _StopBits);
                        serialPort.Data.ReadTimeout = timeOut;
                        serialPort.Data.WriteTimeout = timeOut;
                        serialPort.Data.Open();
                    }
                }
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }
        /// <summary>
        /// 接收
        /// </summary>
        /// <returns></returns>
        public byte[]? Receive()
        {
            try
            {
                using (serialPort.Write())
                {
                    if (Connected)
                    {

                        byte[] buffer = new byte[serialPort.Data.ReadBufferSize];
                        int len = serialPort.Data.Read(buffer, 0, buffer.Length);
                        return buffer.Take(len).ToArray();
                    }
                    return null;
                }
            }
            catch (Exception)
            {
                return null;
            }
        }
        /// <summary>
        /// 发送
        /// </summary>
        /// <param name="buffer"></param>
        /// <returns></returns>
        public bool Send(byte[] buffer)
        {
            try
            {
                using (serialPort.Write())
                {
                    if (Connected)
                    {
                        serialPort.Data.Write(buffer, 0, buffer.Length);
                        return true;
                    }
                    return false;
                }
            }
            catch (Exception)
            {
                return false;
            }
        }
        /// <summary>
        /// 发送并响应
        /// </summary>
        /// <param name="buffer"></param>
        /// <returns></returns>
        public byte[]? SendWait(byte[] buffer)
        {
            for (int i = 0; i < 5; i++)
            {
                if (Send(buffer))
                {
                    Thread.Sleep(10);
                    return Receive();
                }
                else
                {
                    Close();
                    Connect();
                };
                Thread.Sleep(100);
            };
            return null;
        }
        /// <summary>
        /// 关闭连接
        /// </summary>
        public void Close()
        {
            using (serialPort.Write())
            {
                serialPort.Data?.Close();
                serialPort.Data?.Dispose();
                serialPort.Data = null;
            }
        }
        /// <summary>
        /// 释放
        /// </summary>
        public void Dispose()
        {
            Close();
            GC.SuppressFinalize(this);
        }


        /// <summary>
        /// 驱动连接状态
        /// </summary>
        public virtual bool DriverState => Connected;
        /// <summary>
        /// 一个字单位的数据表示的地址长度，西门子为2，三菱，欧姆龙，modbusTcp就为1，AB PLC无效<br />
        /// The address length represented by one word of data, Siemens is 2, Mitsubishi, Omron, modbusTcp is 1, AB PLC is invalid
        /// </summary>
        /// <remarks>
        /// 对设备来说，一个地址的数据对应的字节数，或是1个字节或是2个字节，通常是这两个选择
        /// </remarks>
        public ushort WordLength => _WordLength;
        internal ushort _WordLength = 1;
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
    }
}