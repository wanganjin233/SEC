using SEC.Driver;
using SEC.Util;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;

namespace SEC.DriverService
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;

        public Worker(ILogger<Worker> logger)
        {
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await Task.Run(() =>
            {
                //if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                //{ 
                string address = "127.0.0.1:800";//Environment.GetEnvironmentVariable("address", EnvironmentVariableTarget.Process);
                string EQU ="test";//Environment.GetEnvironmentVariable("EQU", EnvironmentVariableTarget.Process);
                if (!string.IsNullOrEmpty(address))
                {
                    SocketClient socketClient = new(address)
                    {
                        HeadBytes = new byte[2] { 0, 1 },
                        EndBytes = new byte[2] { 2, 3 },
                        HeartbeatBytes = new byte[4] { 0, 1, 2, 3 },
                    };
                    socketClient.ReceiveEvent += SocketClient_ReceiveEvent;
                    socketClient.Connect();
                    socketClient.Send(new OperateResult<string>()
                    {
                        IsSuccess = true,
                        Message = "GetConfig",
                        Content= EQU,
                        TimeSpan = DateTime.UtcNow
                    }.ToJson());

                }
                // }
            }, stoppingToken);
        }

        private void SocketClient_ReceiveEvent(Socket socket, byte[] bytes)
        { 
            var operateResult = bytes.ToString(Encoding.UTF8).ToObject<OperateResult<object>>();
            if (operateResult?.IsSuccess??false)
            {
                switch (operateResult.Message)
                {
                    case "Config":
                        if (operateResult.Content is  string config)
                        {
                            if (config.TryToObject(out EquInfo? _EQUValue))
                            {
                                string? ConnectionString = _EQUValue?.ConnectionString;
                                if (ConnectionString != null)
                                {
                                  //  _EQUValue.DriverType
                                  //  modbusRtu = new OpcUa(ConnectionString);
                                  //  modbusRtu.AddTags(_EQUValue.Tags);
                                  //  this.dataGridView1.AutoGenerateColumns = false;
                                  //  dataGridView1.DataSource = _EQUValue.Tags;
                                  //  Tag? TotalQTY = modbusRtu?.AllTags.Find(p => p.TagName == "TotalQTY");
                                  //  if (TotalQTY != null)
                                  //  {
                                  //      TotalQTY.ValueChangeEvent += UpdateData;
                                  //  }
                                  //  modbusRtu?.AllTags.ForEach(p =>
                                  //  {
                                  //      p.ValueChangeEvent += new Tag.ValueChangeDelegate((Tag tag) =>
                                  //      {
                                  //          Addlog(0, tag.Description + "       " + tag.Value);
                                  //      });
                                  //  });
                                  //  modbusRtu?.Start();
                                }
                            }
                             
                        } 
                        break;
                    default:
                        break;
                } 
            } 
        }

    }
}