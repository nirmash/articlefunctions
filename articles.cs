using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace Articles.get
{
    public class Articles
    {
        private readonly ILogger _logger;

        public Articles(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<Articles>();
        }
        static readonly HttpClient client = new HttpClient();

        private static readonly string NYTimesKeyCredential  = Environment.GetEnvironmentVariable("NYT_SECRET") ?? "SETENVVAR!";
        private static readonly Uri NYTimesEndpoint = new Uri(Environment.GetEnvironmentVariable("NYT_ENDPOINT") ?? "SETENVVAR!");


        [Function("articles")]
        [QueueOutput("urls", Connection = "StorageConnectionString")]
        public static async Task<List<string>> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequestData req, FunctionContext context)
        {

            var nyt_url = NYTimesEndpoint + NYTimesKeyCredential;

            var logger = context.GetLogger("articles");
            logger.LogInformation("Function started processing a request.");

            string responseBody = await client.GetStringAsync(nyt_url);          
            JsonNode articlesObject = JsonNode.Parse(responseBody)!;
            JsonArray articles = articlesObject["results"] as JsonArray;
            if (articles.Count == 0)
            {
                logger.LogInformation("No articles found.");
                return new List<string>();
            }

            List<string> urls = new List<string>();
            var iCount = 0;
            foreach (var article in articles)
            {
                if (article == null)
                    break;
                urls.Add(article["short_url"].ToString());
                iCount++;
                if (iCount == 10)
                {
                    break;
                }
            } 
            return urls;
        }
    }
}
