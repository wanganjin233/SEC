using Microsoft.AspNetCore.Mvc; 
using SEC.Driver; 

namespace SEC.DockerSupporter.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class DriverUpdaterController : ControllerBase
    {
        private readonly ILogger<DriverUpdaterController> _logger;
        private readonly IWebHostEnvironment webHostEnvironment;
        public DriverUpdaterController(ILogger<DriverUpdaterController> logger,
            IWebHostEnvironment webHostEnvironment)
        {
            this.webHostEnvironment = webHostEnvironment;
            _logger = logger;
        }
        /// <summary>
        /// 接收驱动文件
        /// </summary>
        /// <returns></returns>
        [HttpPost(Name = "UploadDriver")]
        public ActionResult UploadDriver()
        {
            try
            {
                IFormFileCollection files = Request.Form.Files;
                List<string> messages = new List<string>();
                foreach (IFormFile file in files)
                {
                    string directory = Path.Combine(webHostEnvironment.ContentRootPath, "Driver");
                    string filePath = Path.Combine(directory, file.FileName);
                    if (!System.IO.File.Exists(filePath))
                    {
                        // 写入文件
                        using var stream = new FileStream(filePath, FileMode.Create);
                        file.CopyTo(stream);
                        stream.Flush();
                        messages.Add($"{file.FileName}上传成功");
                    }
                    else
                    {
                        messages.Add($"{file.FileName}已存在");
                    }
                }
                return Ok(new OperateResult<string>()
                {
                    IsSuccess = true,
                    Message = string.Join(",", messages),
                    TimeSpan = DateTime.UtcNow
                });
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }
    }
}