using SEC.DockerSupporter;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHostedService((sp) => sp.GetRequiredService<ContainerManage>());
builder.Services.AddHostedService((sp) => sp.GetRequiredService<DriverManage>());

builder.Services.AddSingleton<DriverManage>();
builder.Services.AddSingleton<ContainerManage>();
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
