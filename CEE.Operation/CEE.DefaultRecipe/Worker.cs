using SEC.CommUtil;
using SEC.Models.Driver;
using SEC.Util;

namespace CEE.DefaultRecipe
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly SocketClientHelper _sockerClient;

        public Worker(ILogger<Worker> logger, SocketClientHelper sockerClient)
        {
            _logger = logger;
            _sockerClient = sockerClient;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await Task.Run(() =>
              {
                  //全部点位
                  var allTag = DriversCommands.GetAllTag(_sockerClient);
                  //订阅点位
                  Tag? setRecipeResult = CoreCommands.SubTags(_sockerClient,
                        new List<string> {
                          "SetRecipeResult"
                        }
                        )?.FirstOrDefault();

                  //配方设定值
                  var recipeTag = allTag?.FindAll(p => p.TagName.StartsWith("Recipe"));
                  //配方设置开始命令
                  var setRecipeStart = allTag?.FirstOrDefault(p => p.TagName == "SetRecipeStart");

                  Tag? SECRecipe = CoreCommands.SubMQs(_sockerClient,
                      new List<string> {
                          "MES.IssuedRecipe.Base/SECRecipe"
                      }).FirstOrDefault();
                

                  // CoreCommands.PubMQ(_sockerClient, "RecipeIssuedResult.topic/SECResult", new
                  // {
                  //     RepID = Guid.NewGuid(),
                  //     State = 1
                  // }.ToJson()); 
              }, stoppingToken);

            //  bool ss = CoreCommands.PubMQ(_sockerClient, "MES.IssuedRecipe.Base", "sss");



            // foreach (Tag tag in tags)
            // {
            //     tag.ValueChangeEvent += Tag_ValueChangeEvent;
            // }
            // tags.First().Value = 12;
            // DriversCommands.WriteTag(_sockerClient, tags.First());
            // //List<Tag> tags1 = CoreCommands.SubMQs(_sockerClient, new List<string> { "MES.IssuedRecipe.Base/tes22" });
            // //foreach (Tag tag in tags1)
            // //{
            // //    tag.ValueChangeEvent += Tag_ValueChangeEvent;
            // //}
        }

        private void Tag_ValueChangeEvent(Tag tag)
        {
            Console.WriteLine("name:" + tag.Value);
        }
    }
}