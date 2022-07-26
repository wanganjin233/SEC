namespace SEC.Drivers.MC3EEthernetDriver
{
    /// <summary>
    /// 寄存器地址类型
    /// </summary>
    public enum AddressTypeEnum
    {
        /// <summary>
        /// 辅助寄存器 
        /// </summary> 
        M = 0x90,
        /// <summary>
        /// 输入继电器 
        /// </summary> 
        X = 0x9C,
        /// <summary>
        /// 输出继电器 
        /// </summary> 
        Y = 0x9D,
        /// <summary>
        /// 锁存继电器 
        /// </summary> 
        L = 0x92,
        /// <summary>
        /// 报警继电器 
        /// </summary> 
        F = 0x93,
        /// <summary>
        /// 边沿继电器 
        /// </summary> 
        V = 0x94,
        /// <summary>
        /// 链接继电器 
        /// </summary> 
        B = 0xA0,
        /// <summary>
        /// 定时器触点 
        /// </summary> 
        TS = 0xC1,
        /// <summary>
        /// 定时器当前值 
        /// </summary> 
        TC = 0xC2,
        /// <summary>
        /// 累计定时器触点  
        /// </summary> 
        SS = 0xC7,
        /// <summary>
        /// 累计定时器线圈
        /// </summary> 
        SC = 0xC6,
        /// <summary>
        /// 计数器触点
        /// </summary>
        CS = 0xC4,
        /// <summary>
        /// 计数器线圈
        /// </summary>
        CC = 0xC3,
        /// <summary>
        /// 步进继电器
        /// </summary>
        S = 0x98,
        /// <summary>
        /// 数据寄存器
        /// </summary>
        D = 0xA8,
        /// <summary>
        /// 链接寄存器
        /// </summary>
        W = 0xB4,
        /// <summary>
        /// 定时器线圈
        /// </summary> 
        TN = 0xC0,
        /// <summary>
        /// 累计定时器当前值
        /// </summary> 
        SN = 0xC8,
        /// <summary>
        /// 计数器当前值
        /// </summary> 
        CN = 0xC5,
        /// <summary>
        /// 变址寄存器
        /// </summary> 
        Z = 0xCC,
        /// <summary>
        /// 文件寄存器
        /// </summary> 
        R = 0xAF,
        /// <summary>
        /// 文件寄存器
        /// </summary> 
        ZR = 0xB0
    }

}