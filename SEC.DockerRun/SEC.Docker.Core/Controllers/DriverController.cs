using Microsoft.AspNetCore.Mvc;
using System.Data;
using SEC.Util;
using System.Text;
using SEC.Models.Interactive;
using Docker.DotNet.Models;
using Dapper.Contrib.Extensions;

namespace SEC.Docker.Core.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class DriverController : ControllerBase
    {
        private readonly ILogger _logger;
        SqliteHelper sqliteHelper;
        DockerManage dockerManage;
        DriverManage driverManage;
        public DriverController(ILogger<DriverController> logger, SqliteHelper sqliteHelper, DockerManage dockerManage, DriverManage driverManage)
        {
            this.sqliteHelper = sqliteHelper;
            this.dockerManage = dockerManage;
            this.driverManage = driverManage;
            _logger = logger;
        }
        /// <summary>
        /// 上传设备配置文件
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="fileVersions"></param>
        /// <param name="files"></param>
        /// <returns></returns>
        [HttpPost(Name = "UploadConfig")]
        public ActionResult UploadConfig([FromForm] string driverConfigJson, [FromForm] IFormFileCollection files)
        {
            StringBuilder messger = new StringBuilder();
            if (driverConfigJson.TryToObject(out List<EquConfig>? driverConfigs))
            {
                using var connection = sqliteHelper.Connection;
                var equStates = connection.GetAll<EquState>();
                var equConfigs = connection.GetAll<EquConfig>();


                foreach (var driverConfig in driverConfigs)
                {
                    IFormFile? formFile = files.FirstOrDefault(p => p.FileName == driverConfig.ConfigPath);
                    if (formFile != null)
                    {
                        if (equConfigs.Where(p => p.Equ == driverConfig.Equ && p.Version == driverConfig.Version).Any())
                        {
                            messger.AppendLine($"设备【{driverConfig.Equ}】【{driverConfig.Version}】版本的配置文件已存在");
                        }
                        else if (Path.GetExtension(formFile.FileName).ToLower() != ".json")
                        {
                            messger.AppendLine($"驱动配置【{driverConfig.Equ}】上传失败，格式必须为json");
                        }
                        else
                        {
                            using IDbTransaction dbTransaction = connection.BeginTransaction();
                            try
                            {
                                string driverName = Guid.NewGuid().ToString("N") + ".json";
                                string directory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Properties");
                                string filePath = Path.Combine(directory, driverName);
                                if (!equStates.Any(p => p.Equ == driverConfig.Equ))
                                {
                                    connection.Insert(
                                        new EquState
                                        {
                                            Id = Guid.NewGuid().ToString("N"),
                                            Enable = false,
                                            Equ = driverConfig.Equ,
                                            EquName = Path.GetFileNameWithoutExtension(driverConfig.ConfigPath),
                                            UpdateTime = DateTime.UtcNow,
                                        }, dbTransaction);
                                }
                                driverConfig.Id = Guid.NewGuid().ToString("N");
                                driverConfig.ConfigPath = filePath;
                                driverConfig.UpdateTime = DateTime.UtcNow;
                                connection.Insert(driverConfig, dbTransaction);
                                if (!System.IO.File.Exists(filePath))
                                {
                                    using var stream = new FileStream(filePath, FileMode.Create);
                                    formFile.CopyTo(stream);
                                    stream.Flush();
                                    dbTransaction.Commit();
                                    messger.AppendLine($"配置文件【{driverConfig.Equ}】版本【{driverConfig.Version}】已上传");
                                }
                                else
                                {
                                    messger.AppendLine($"配置文件【{driverConfig.Equ}】版本【{driverConfig.Version}】文件已存在");
                                }
                            }
                            catch (Exception e)
                            {
                                dbTransaction.Rollback();
                                messger.AppendLine($"配置文件【{driverConfig.Equ}】版本【{driverConfig.Version}】上传异常【{e.Message}】");
                            }
                        }
                    }
                }
            }
            string _messger = messger.ToString();
            _logger.LogInformation(_messger);
            return Ok(messger.ToString());
        }

        /// <summary>
        /// 获取驱动运行状态
        /// </summary>
        /// <param name="driverInfos"></param>
        /// <returns></returns>
        [HttpPost(Name = "DriverState")]
        public async Task<ActionResult> DriverState()
        {
            List<DriverInfo> driverInfos = new List<DriverInfo>();
            IList<ContainerListResponse> containers = await dockerManage.GetContainersAsync();
            using (IDbConnection db = sqliteHelper.Connection)
            {
                List<EquState> driverConfigs = db.GetAll<EquState>().ToList();
                foreach (var driverConfig in driverConfigs)
                {
                    DriverInfo driverInfo = new DriverInfo { Name = driverConfig.Equ, Enable = driverConfig.Enable };
                    var container = containers.FirstOrDefault(p => p.Names.Any(x => x == $"/{driverInfo.Name}"));
                    if (container != null)
                    {
                        driverInfo.IPAddress = container.NetworkSettings.Networks.ToDictionary(p => p.Key, p => p.Value.IPAddress);
                        driverInfo.State = container.State;
                        driverInfo.Status = container.Status;
                    }
                    driverInfos.Add(driverInfo);
                }
            }
            return Ok(driverInfos);
        }
        /// <summary>
        /// 停用驱动 
        /// </summary>
        /// <param name="driverInfos"></param>
        /// <returns></returns>
        [HttpPost(Name = "DisabledDriver")]
        public async Task<ActionResult> DisabledDriver(List<string> drivers)
        {
            StringBuilder messagers = new StringBuilder();
            IList<ContainerListResponse> containers = await dockerManage.GetContainersAsync();
            using (IDbConnection db = sqliteHelper.Connection)
            {
                List<EquState> equStates = db.GetAll<EquState>().Where(p => p.Enable && drivers.Any(x => x == p.Equ)).ToList();
                if (!equStates.Any())
                {
                    string messager = $"未找到要停止的设备";
                    _logger.LogError(messager);
                    return BadRequest(messager);
                }
                foreach (var equState in equStates)
                {
                    equState.Enable = false;
                    using IDbTransaction dbTransaction = db.BeginTransaction();
                    try
                    {
                        if (db.Update(equState, dbTransaction))
                        {
                            var container = containers.FirstOrDefault(p => p.Names.Any(x => x == $"/{equState.Equ}"));
                            if (container == null || await dockerManage.StopContainerAsync(container.ID))
                            {
                                dbTransaction.Commit();
                                string messager = $"【{equState.Equ}】 驱动已停用";
                                _logger.LogInformation(messager);
                                messagers.AppendLine(messager);
                            }
                            else
                            {
                                dbTransaction.Rollback();
                                string messager = $"【{equState.Equ}】 驱动未停用";
                                _logger.LogError(messager);
                                messagers.AppendLine(messager);
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        dbTransaction.Rollback();
                        string messager = $"【{equState.Equ}】 驱动未停用 【{e.Message}】";
                        _logger.LogError(messager);
                        messagers.AppendLine(messager);

                    }
                }
            }
            return Ok(messagers.ToString());
        }
        /// <summary>
        /// 启用驱动 
        /// </summary>
        /// <param name="driverInfos"></param>
        /// <returns></returns>
        [HttpPost(Name = "EnableDriver")]
        public async Task<ActionResult> EnableDriver(List<string> drivers)
        {
            StringBuilder messagers = new StringBuilder();
            IList<ContainerListResponse> containers = await dockerManage.GetContainersAsync();
            using (IDbConnection db = sqliteHelper.Connection)
            {
                List<EquState> equStates = db.GetAll<EquState>().Where(p => drivers.Any(x => x == p.Equ)).ToList();
                if (!equStates.Any())
                {
                    string messager = $"未找到要启用的设备";
                    _logger.LogError(messager);
                    return BadRequest(messager);
                }
                using IDbTransaction dbTransaction = db.BeginTransaction();
                equStates.ForEach(p =>
                {
                    p.Enable = true;
                    p.UpdateTime = DateTime.UtcNow;
                });
                if (db.Update(equStates, dbTransaction))
                {
                    try
                    {
                        await driverManage.EnableDriverAsync(equStates);
                        dbTransaction.Commit();
                        string messager = $"驱动已启用";
                        _logger.LogInformation(messager);
                        return Ok(messager);
                    }
                    catch (Exception e)
                    {

                        dbTransaction.Rollback();
                        string messager = $"驱动未启用【{e.Message}】";
                        _logger.LogError(messager);
                        return BadRequest(messager);
                    }
                }
                else
                {
                    string messager = $"更新状态失败";
                    _logger.LogError(messager);
                    return BadRequest(messager);
                }

            }
        }
    }
}