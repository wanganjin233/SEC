namespace SEC.Docker.Core
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(); 
            builder.Services.AddLogging(builder =>
            {
                builder.SetMinimumLevel(LogLevel.Trace);
                builder.AddProvider(new KafkaLoggerProvider());
            });
            builder.Services.AddHostedService((sp) => sp.GetRequiredService<DriverManage>());
            builder.Services.AddSingleton<DriverManage>();
            builder.Services.AddSingleton(new DockerManage());
            builder.Services.AddSingleton<KafkaHelper>();
            builder.Services.AddSingleton<SqliteHelper>();
            
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