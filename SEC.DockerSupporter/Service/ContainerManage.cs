using Docker.DotNet;
using Docker.DotNet.Models;
using Docker.DotNet.X509;
using SEC.Util;
using System.Security.Cryptography.X509Certificates;
using System.Security.Policy;

namespace SEC.DockerSupporter
{
    public class ContainerManage : BackgroundService
    {

        public DockerClient DockerClientX509Certificate(string url, string certificateKeyPath, string password)
        {
            var credentials = new CertificateCredentials(new X509Certificate2(certificateKeyPath, password));
            credentials.ServerCertificateValidationCallback += (o, c, ch, er) => true;
            var config = new DockerClientConfiguration(new Uri(url), credentials);
            return config.CreateClient();
        }


        public ContainerManage()
        { 
            var config = new DockerClientConfiguration();
            DockerClient client = config.CreateClient();
            var asd = Task.Run(async () =>
                {
                    //   await client.Images.CreateImageAsync(
                    // new ImagesCreateParameters { FromSrc = "https://registry.docker-cn.com", FromImage = "mysql", Tag = "latest" }, null,
                    // new Progress<JSONMessage>(message =>
                    // {
                    //     var sada = !string.IsNullOrEmpty(message.ErrorMessage)
                    //         ? message.ErrorMessage
                    //         : $"{message.ID} {message.Status} {message.ProgressMessage}";
                    // }));
                    //

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


            foreach (var item in asd)
            {
                Console.WriteLine(item.ID);
            }
 


        }
        /// <summary>
        /// 初始化任务
        /// </summary>
        /// <param name="stoppingToken"></param>
        /// <returns></returns>
        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        { 
            return Task.FromResult(0);
        }

    }
}

