using Docker.DotNet;
using Docker.DotNet.Models;
using Docker.DotNet.X509;
using Microsoft.Win32; 
using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Loader;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using System.Windows;

namespace SEC.Studio
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
         //   InitializeComponent();
        }
        [MethodImpl(MethodImplOptions.NoInlining)]
        public Type  Load(out WeakReference weakReference)
        {

            AssemblyLoadContext? assemblyLoadContext = new AssemblyLoadContext("ModbusTcp", true);
            weakReference = new WeakReference(assemblyLoadContext);
            Assembly assembly = assemblyLoadContext.LoadFromAssemblyPath(@"D:\SEC\SEC.Drivers\SEC.ModebusTcpDriver\bin\Debug\net6.0\SEC.Driver.ModebusTcp.dll");
            var asd = assembly.GetType("SEC.Driver.ModebusTcp.ModbusTcp");
            SocketClient tCPClient = new SocketClient("127.0.0.1:200");
            BaseDriver? asda = (BaseDriver?)Activator.CreateInstance(asd, new object[] { tCPClient }); 
            assemblyLoadContext.Unload(); 
            assemblyLoadContext = null; 
            return asd;
        }

        private void OpenProject(object sender, RoutedEventArgs e)
        {
            
                 
                var credentials = new CertificateCredentials(new X509Certificate2(@"C:\Users\su\source\repos\sec\SEC.Studio\Images\key.pfx", "asdfghjkl"));
                credentials.ServerCertificateValidationCallback += (o, c, ch, er) => true;
                var config = new DockerClientConfiguration(new Uri("http://manage.neung.top:2375"), credentials);
                DockerClient client = config.CreateClient();
            
            var asd=    Task.Run(async () =>
                  {
                      await client.Containers.CreateContainerAsync(new CreateContainerParameters()
                      {
                          Image = "fedora/memcached", 
                          HostConfig = new HostConfig()
                          {
                              DNS = new[] { "8.8.8.8", "8.8.4.4" } 
                          }
                      });
            
                      return await client.Images.ListImagesAsync(new ImagesListParameters() { All = true }); 
                  }).Result;




            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "数据文件|*.s3db|数据文件|*.DB";
            if (openFileDialog.ShowDialog() == true)
            {
                new Project().Show();
                Close();
            }
            else
            {

            }
        }
    }
}
