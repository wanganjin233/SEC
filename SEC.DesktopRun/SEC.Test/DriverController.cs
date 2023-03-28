using Microsoft.AspNetCore.Mvc;
using System.Data;
using SEC.Util;
using System.Text;
using System.Data.Common;
using System.Transactions;
using Confluent.Kafka;
using System.Collections.Generic;
using System.Linq;
using Dapper.Contrib.Extensions;
using SEC.Models.Interactive;
using Microsoft.AspNetCore.Http;

namespace SEC.Test
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class DriverController : ControllerBase
    { 
        public DriverController( )
        {  
        }
        /// <summary>
        /// 上传设备配置文件
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="fileVersions"></param>
        /// <param name="files"></param>
        /// <returns></returns>
        [HttpPost(Name = "UploadConfig")]
        public ActionResult UploadConfig([FromForm] IFormFileCollection files)
        {
            StringBuilder messger = new StringBuilder();
            foreach (var file in files)
            {
                string driverName = file.FileName;
                string directory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Config");
                string filePath = Path.Combine(directory, driverName);
                try
                {
                    if (!System.IO.File.Exists(filePath))
                    {
                        using var stream = new FileStream(filePath, FileMode.Create);
                        file.CopyTo(stream);
                        stream.Flush();
                        messger.AppendLine($"配置文件【{driverName}】 已上传");
                    }
                    else
                    {
                        messger.AppendLine($"配置文件【{driverName}】 文件已存在");
                    }
                }
                catch (Exception e)
                {
                    messger.AppendLine($"配置文件【{driverName}】 上传异常【{e.Message}】");
                }
            }
            string _messger = messger.ToString(); 
            return Ok(messger.ToString());
        }

        /// <summary>
        /// 获取驱动运行状态
        /// </summary>
        /// <param name="driverInfos"></param>
        /// <returns></returns>
        [HttpPost(Name = "DriverState")]
        public ActionResult DriverState()
        {
            List<DriverInfo> driverInfos = new();
            string[] allConfgs = Directory.GetFiles(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Config"));
            string[] enableEqus = System.IO.File.ReadAllLines(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "EnableEqus"));
            foreach (string conf in allConfgs)
            { 
                driverInfos.Add(new DriverInfo
                {
                    Name = conf,
                    Enable = enableEqus.Contains(Path.GetFileNameWithoutExtension(conf)) 
                }); 
            }
            return Ok(driverInfos);
        }
        /// <summary>
        /// 停用驱动 
        /// </summary>
        /// <param name="driverInfos"></param>
        /// <returns></returns>
        [HttpPost(Name = "DisabledDriver")]
        public  ActionResult DisabledDriver(List<string> equs)
        { 
            StringBuilder messagers = new StringBuilder();

            return Ok(messagers);
        }
        /// <summary>
        /// 启用驱动 
        /// </summary>
        /// <param name="driverInfos"></param>
        /// <returns></returns>
        [HttpPost(Name = "EnableDriver")]
        public ActionResult  EnableDriver(List<string> drivers)
        {
            StringBuilder messagers = new StringBuilder(); 
                return Ok(messagers);
                 
        }
    }
}