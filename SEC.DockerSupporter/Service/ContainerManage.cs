using Dapper.Contrib.Extensions; 
using MySql.Data.MySqlClient;
using SEC.Driver;
using SEC.Util;
using System.Data; 

namespace SEC.DockerSupporter
{
    public class ContainerManage : BackgroundService
    { 
        public ContainerManage( )
        { 
            var connectionString = new ConfigurationBuilder()
                .SetBasePath(Directory
                .GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build()
                .GetSection("ConnectionString").Value; 
            IDbConnection conn = new MySqlConnection(connectionString); 
            conn.Open();
            var asd = conn.GetAll<EquInfo>() ;
             



        }

        [Table("aaa")]
        public class aaa
        {
            [Key]
            public string? aa { get; set; }  
            
            /// <summary>
            /// 名称
            /// </summary>
            public string? bb { get; set; }  
            
        }
            /// <summary>
            /// 初始化任务
            /// </summary>
            /// <param name="stoppingToken"></param>
            /// <returns></returns>
            protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
          


                var ad=  IpHelper.GetLocalIps();
            return Task.FromResult(0);
        }
        
    }
}

