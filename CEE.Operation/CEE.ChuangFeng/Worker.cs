using CEE.Models;
using SEC.Communication;
using SEC.CommUtil;
using SEC.Models.Driver;
using SEC.Util;

namespace CEE.ChuangFeng
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly SocketClientHelper _sockerClient;
        public Worker(ILogger<Worker> logger,  SocketClientHelper sockerClient)
        {
            _logger = logger;
            _sockerClient= sockerClient;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {

            Tag? ChuangFengIssued = CoreCommands.SubMQs(_sockerClient,
                    new List<string> {
                          "MES.IssuedRecipe.Base/ChuangFengIssued"
                    }).FirstOrDefault();
            if (ChuangFengIssued!=null)
            {
                ChuangFengIssued.ValueChangeEvent += ChuangFengIssued_ValueChangeEvent;
            }

           // CoreCommands.PubMQ(_sockerClient, "RecipeIssuedResult.topic/ChuangFengResult")
            


            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                await Task.Delay(1000, stoppingToken);
            }
        } 
        private void ChuangFengIssued_ValueChangeEvent(Tag tag)
        {
            if (tag.Value?.ToString()?.TryToObject(out IssuedRecipe? issuedRecipe)??false)
            {
                

            } 
        }
    }
}