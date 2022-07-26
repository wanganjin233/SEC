using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SEC.ModebusTcpDriver
{

    /// <summary>
    /// 寄存器地址类型
    /// </summary>
    public enum AddressTypeEnum
    {
        /// <summary>
        /// 输出线圈
        /// </summary> 
        [Description("0")]
        zero = 0x0,
        /// <summary>
        /// 输入线圈
        /// </summary> 
        [Description("1")]
        one = 0x01,
        /// <summary>
        /// 内部寄存器
        /// </summary> 
        [Description("3")]
        threea = 0x04,
        /// <summary>
        /// 保持寄存器
        /// </summary> 
        [Description("3")]
        four = 0x03
    }
}
