using Docker.DotNet;
using Docker.DotNet.X509;
using System.Security.Cryptography.X509Certificates;
using Docker.DotNet.Models;
using SEC.Enum.Docker;

namespace SEC.Docker.Core
{
    public class DockerManage
    {
        DockerClient dockerClient;
        #region 创建连接
        /// <summary>
        /// 用证书验证连接
        /// </summary>
        /// <param name="url">地址</param>
        /// <param name="certificateKeyPath">证书路径</param>
        /// <param name="password">密码</param>
        /// <returns></returns>
        public DockerManage(string url, string certificateKeyPath, string password)
        {
            Credentials credentials = new CertificateCredentials(new X509Certificate2(certificateKeyPath, password));
            var config = new DockerClientConfiguration(new Uri(url), credentials);
            dockerClient = config.CreateClient();
        }
        /// <summary>
        /// 创建docker客户端
        /// </summary>
        /// <returns></returns>
        public DockerManage()
        {
            dockerClient = new DockerClientConfiguration().CreateClient();
        }
        /// <summary>
        /// 创建docker客户端
        /// </summary>
        /// <returns></returns>
        public DockerManage(string url)
        {
            dockerClient = new DockerClientConfiguration(new Uri(url)).CreateClient();
        }
        #endregion
        #region 操作容器
        /// <summary>
        /// 获取所有容器
        /// </summary>
        /// <param name="dockerClient"></param>
        /// <returns></returns>
        public async Task<IList<ContainerListResponse>> GetContainersAsync()
        {
            return await dockerClient.Containers.ListContainersAsync(new ContainersListParameters { All = true });
        }

        /// <summary>
        /// 创建容器
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        ///<summary>
        ///</summary>
        ///<param name=""></param>
        public async Task<CreateContainerResponse> CreateContainerAsync(
              string name,
              string fromImage,
              string tag = "latest",
              List<string>? binds = null,
              List<string>? envs = null,
              List<string>? volumes = null,
              List<string>? endpoints = null,
              List<string>? ports = null
            )
        {
            CreateContainerParameters createContainerParameters = new CreateContainerParameters
            {
                Image = $"{fromImage}:{tag ?? "latest"}",
                Name = name
            };
            createContainerParameters.HostConfig = new HostConfig()
            {
                RestartPolicy = new RestartPolicy { Name = RestartPolicyKind.Always }
            };
            //绑定
            if (binds != null)
            {
                createContainerParameters.HostConfig.Binds = binds;
            }
            //挂载
            if (volumes != null)
            {
                createContainerParameters.Volumes = volumes.ToDictionary(x => x, x => new EmptyStruct());
            }
            //参数
            if (envs != null)
            {
                createContainerParameters.Env = envs;
            }
            //使用的网络
            if (endpoints != null)
            {
                createContainerParameters.NetworkingConfig = new NetworkingConfig
                {
                    EndpointsConfig = endpoints.ToDictionary(x => x, x => new EndpointSettings())
                };
            }
            //绑定端口
            if (ports != null)
            {
                Dictionary<string, IList<PortBinding>> portBindings = new Dictionary<string, IList<PortBinding>>();
                foreach (var port in ports)
                {
                    string[] portSplit = port.Split(":");
                    string exposedPort = portSplit[0];
                    string hostPort = portSplit[1];
                    if (portBindings.ContainsKey(hostPort))
                    {
                        portBindings[hostPort].Add(new PortBinding { HostIP = "localhost", HostPort = exposedPort });
                    }
                    else
                    {
                        portBindings.Add(hostPort, new List<PortBinding> { new PortBinding { HostIP = "localhost", HostPort = exposedPort } });
                    }
                }
                createContainerParameters.ExposedPorts = portBindings.ToDictionary(x => x.Key, x => new EmptyStruct());
                createContainerParameters.HostConfig.PortBindings = portBindings;
            }

            return await dockerClient.Containers.CreateContainerAsync(createContainerParameters);
        }
        /// <summary>
        /// 启动容器
        /// </summary>
        public async Task<bool> StartContainerAsync(string id)
        {
            return await dockerClient.Containers.StartContainerAsync(id, new ContainerStartParameters { });
        }
        /// <summary>
        /// 停止容器
        /// </summary>
        public async Task<bool> StopContainerAsync(string id)
        {
            return await dockerClient.Containers.StopContainerAsync(id, new ContainerStopParameters { });
        }
        #endregion
        #region 操作镜像
        /// <summary>
        /// 获取所有镜像
        /// </summary>
        /// <param name="dockerClient"></param>
        /// <returns></returns>
        public async Task<IList<ImagesListResponse>> GetImagesAsync()
        {
            return await dockerClient.Images.ListImagesAsync(new ImagesListParameters { All = true });
        }
        /// <summary>
        /// 拉取镜像
        /// </summary>
        /// <param name="dockerClient"></param>
        /// <returns></returns>
        public async Task PullImageAsync(string fromImage, string tag = "latest", Action<JSONMessage>? action = null)
        {
            await dockerClient.Images.CreateImageAsync(
             new ImagesCreateParameters
             {
                 FromImage = fromImage,
                 Tag = tag
             }, null, action == null ? new Progress<JSONMessage>() : new Progress<JSONMessage>(action)
             );
        }
        #endregion
        #region 操作网络
        /// <summary>
        /// 获取网络
        /// </summary>
        /// <param name="dockerClient"></param>
        /// <returns></returns>
        public async Task<IList<NetworkResponse>> GetNetworksAsync()
        {
            return await dockerClient.Networks.ListNetworksAsync(new NetworksListParameters());
        }
        /// <summary>
        /// 创建网络
        /// </summary>
        /// <param name="dockerClient"></param>
        /// <param name="name"></param>
        /// <param name="driver"></param>
        /// <returns></returns>
        public async Task<NetworksCreateResponse> CreateNetworkAsync(string name, NetworkDriver driver)
        {
            return await dockerClient.Networks.CreateNetworkAsync(
                new NetworksCreateParameters()
                {
                    Name = name,
                    Driver = driver.ToString()
                });
        }
        #endregion
        #region 操作卷
        /// <summary>
        /// 获取卷
        /// </summary>
        /// <param name="dockerClient"></param>
        /// <returns></returns>
        public async Task<IList<VolumeResponse>> GetVolumesAsync()
        {
            VolumesListResponse volumesListResponse = await dockerClient.Volumes.ListAsync();
            return volumesListResponse.Volumes;
        }
        /// <summary>
        /// 创建卷
        /// </summary>
        /// <param name="dockerClient"></param>
        /// <returns></returns>
        public async Task<VolumeResponse> CreateVolumesAsync(string name)
        {
            return await dockerClient.Volumes.CreateAsync(new VolumesCreateParameters { Name = name });
        }
        #endregion
    }
}
