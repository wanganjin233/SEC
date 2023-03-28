using SEC.Util;

namespace SEC.CommUtil
{
    public static class Config
    {
        private static string? localIp;
        /// <summary>
        /// 本机ip
        /// </summary>
        public static string LocalIp => localIp ??= IpHelper.GetLocalIp();

        /// <summary>
        /// 启用的设备配置路径
        /// </summary>
        public static string EnableConfigPath => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "EnableConfig");
        /// <summary>
        /// 未启用的设备配置路径
        /// </summary>
        public static string DisabledConfigPath => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "DisabledConfig"); 

    }
}
