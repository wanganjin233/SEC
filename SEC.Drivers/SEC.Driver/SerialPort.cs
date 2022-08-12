using SEC.Util;
using System.IO.Ports;

namespace SEC.Driver
{
    public class SerialPort : ICommunication
    {
        /// <summary>
        /// 串口对象
        /// </summary>  
        private readonly UsingLock<System.IO.Ports.SerialPort> serialPort = new();
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
        /// 超时时间
        /// </summary>
        private readonly int _TimeOut = 3000;
        public SerialPort(string portName, int baudRate, int dataBits, string parity, string stopBits, int timeOut = 3000)
        {
            _PortName = portName;
            _BaudRate = baudRate;
            _DataBits = dataBits;
            _TimeOut = timeOut;
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
        }
        /// <summary>
        /// 连接
        /// </summary>
        /// <returns></returns>
        public bool Connect()
        {
            try
            {
                Close();
                using (serialPort.Write())
                {
                    if (!Connected)
                    {
                        serialPort.Data = new System.IO.Ports.SerialPort(_PortName, _BaudRate, _Parity, _DataBits, _StopBits);
                        serialPort.Data.DtrEnable = true;
                        serialPort.Data.RtsEnable = true;
                        serialPort.Data.ReadTimeout = _TimeOut;
                        serialPort.Data.WriteTimeout = _TimeOut;
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
    }
}