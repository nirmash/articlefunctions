using System;
using System.Net;
using System.Text.Json.Nodes;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using HtmlAgilityPack;

namespace Articles.Function
{
    public class Extractor
    {
        private readonly ILogger _logger;

        public Extractor(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<Extractor>();
        }
        static HttpClient client = new HttpClient();

        [Function("extractor")]
        [BlobOutput("articles-text-input/{rand-guid}.txt", Connection = "StorageConnectionString")]
        public static async Task<string> Run([QueueTrigger("urls", Connection = "StorageConnectionString")] string myQueueItem, FunctionContext context)
        {
            var logger = context.GetLogger("extractor");
            try{
            logger.LogInformation("Function started processing {myQueueItem}.", myQueueItem);
            var web = new HtmlWeb();
            var doc = await web.LoadFromWebAsync(myQueueItem);
            var textDecoded = doc.DocumentNode.InnerText;
            return HtmlEntity.DeEntitize(textDecoded);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error processing {myQueueItem}.", myQueueItem);
                return "";
            }
        }
    }
}
