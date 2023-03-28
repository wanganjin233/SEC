using SEC.CommUtil;
using SEC.Util;

namespace SEC.CoreRun
{
    public class Program
    {
        public static void Main(string[] args)
        { 
            var builder = WebApplication.CreateBuilder(args);
            
            KafkaHelper kafkaHelper = new(builder.Configuration["KafkaConnection"]);

            var userName = builder.Configuration.GetSection("RabbitMQConnection:UserName").Get<string>();
            var password = builder.Configuration.GetSection("RabbitMQConnection:PassWord").Get<string>();
            var port = builder.Configuration.GetSection("RabbitMQConnection:Port").Get<int>();
            var connections = builder.Configuration.GetSection("RabbitMQConnection:Connections").Get<string[]>(); 
            var rabbitMQHelper = new RabbitMQHelper(userName, password, port, connections); 

            builder.Services.AddSingleton(rabbitMQHelper);
            // Add services to the container. 
            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            builder.Services.AddHostedService<CoreService>();
            builder.Services.AddSingleton<CoreService>();
            builder.Services.AddLogging(p =>
            {
                p.SetMinimumLevel(LogLevel.Trace);
                p.AddProvider(new LoggerHelper((logInfo) =>
                {
                    kafkaHelper.Send("system-service-log", new
                    {
                        Topic = "",
                        Key = "Supporter",
                        Message = new
                        {
                            Category = "Supporter",
                            LogLevel = logInfo.LogLevel,
                            State = logInfo.State,
                            Message = logInfo.Message,
                            ApplicationName = logInfo.CategoryName,
                            Time = DateTime.UtcNow,
                            IpAddress = Config.LocalIp
                        }
                    }.ToJson()); 
                }));
            });
            builder.Services.AddSingleton(kafkaHelper);
            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}