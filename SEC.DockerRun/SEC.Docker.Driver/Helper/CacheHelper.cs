using SEC.Util;
namespace SEC.Docker.Driver
{
    /// <summary>
    /// 缓存操作
    /// </summary>
    public static class CacheHelper
    {
        private static readonly UsingLock<object> UsingLock = new UsingLock<object>();

        private static string GetCachePath(bool Read, bool Desc)
        {
            string cacheFilePath = string.Empty;
            string basePath = Environment.GetEnvironmentVariable("basePath", EnvironmentVariableTarget.Process) ?? AppDomain.CurrentDomain.BaseDirectory;
            string cachePath = Path.Combine(basePath, "Cache"); 
            if (!Directory.Exists(cachePath))
                Directory.CreateDirectory(cachePath);
            Dictionary<string, long> CacheFileTime = new Dictionary<string, long>();
            foreach (var CacheFile in Directory.GetFiles(cachePath))
            { 
                CacheFileTime.Add(CacheFile, long.Parse(Path.GetFileName(CacheFile)));
            }
            if (CacheFileTime.Any())
            {
                if (Desc)
                { 
                    cacheFilePath = CacheFileTime.OrderByDescending(x => x.Value).First().Key;
                    if (File.ReadLines(cacheFilePath).Count() >= 1000)
                    {
                        cacheFilePath = Path.Combine(cachePath, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString());
                    }
                }
                else
                {
                    cacheFilePath = CacheFileTime.OrderBy(x => x.Value).First().Key;
                }
            }
            else if (!Read)
            {
                cacheFilePath = Path.Combine(cachePath, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString());
            }
            return cacheFilePath;
        }
        /// <summary>
        /// 写入
        /// </summary>
        /// <param name="messager"></param>
        public static void Write(string messager)
        {
            using (UsingLock.Write())
            {
                string cachePath = GetCachePath(false, true);
                File.AppendAllLines(cachePath, new List<string> { messager });
            }
        }
        /// <summary>
        /// 读取
        /// </summary>
        /// <returns></returns>
        public static string? Read()
        {
            using (UsingLock.Read())
            {
                string cachePath = GetCachePath(true, false);
                if (string.IsNullOrEmpty(cachePath))
                {
                    return null;
                }
                return File.ReadLines(cachePath).FirstOrDefault();
            }
        }
        /// <summary>
        /// 删除
        /// </summary>
        public static void DeleteFirst()
        {
            using (UsingLock.Write())
            {
                string cachePath = GetCachePath(true, false);
                if (!string.IsNullOrEmpty(cachePath))
                { 
                    var lines = File.ReadAllLines(cachePath);
                    if (lines.Any())
                    {
                        File.WriteAllLines(cachePath, lines.Skip(1));
                    }
                    else
                    {
                        File.Delete(cachePath);
                    }
                }
            }
        }

    }
}
