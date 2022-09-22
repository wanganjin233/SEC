using Docker.DotNet;
using Docker.DotNet.Models;
using Docker.DotNet.X509;
using SEC.Util;
using System.Security.Cryptography.X509Certificates;

namespace SEC.DockerSupporter
{
    public class ContainerManage : BackgroundService
    {
        public ContainerManage()
        {

            var credentials = new CertificateCredentials(new X509Certificate2("D:\\SEC\\SEC.DockerSupporter\\Properties\\key.pfx", "asdfghjkl"));
            credentials.ServerCertificateValidationCallback += (o, c, ch, er) => true;
            var config = new DockerClientConfiguration(new Uri("http://manage.neung.top:2375"), credentials);
            DockerClient client = config.CreateClient(); 
            var asd = Task.Run(async () =>
                  {
                      await client.Images.CreateImageAsync(
                    new ImagesCreateParameters {FromSrc= "https://registry.docker-cn.com", FromImage = "mysql", Tag = "latest" }, null,
                    new Progress<JSONMessage>(message =>
                    {
                        var sada=!string.IsNullOrEmpty(message.ErrorMessage)
                            ? message.ErrorMessage
                            : $"{message.ID} {message.Status} {message.ProgressMessage}" ;
                    }));
                       

                     // await client.Containers.CreateContainerAsync(new CreateContainerParameters()
                     // {
                     //     Image = "fedora/memcached",
                     //     HostConfig = new HostConfig()
                     //     {
                     //         DNS = new[] { "8.8.8.8", "8.8.4.4" }
                     //     }
                     // });
                     // 
                       return await client.Images.ListImagesAsync(new ImagesListParameters() { All = true });
                  }).Result;


        }
        /// <summary>
        /// 初始化任务
        /// </summary>
        /// <param name="stoppingToken"></param>
        /// <returns></returns>
        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {



            var ad = IpHelper.GetLocalIps();
            return Task.FromResult(0);
        }

    }
}

