using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Xml;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace tanabebe.tech.function
{
    public static class TimerBlogNotification
    {
        private static readonly HttpClient HttpClient;
        private static readonly Random Random;
        private static readonly DateTime Now = DateTime.Now;
        private static readonly int Seed = Environment.TickCount;
        private static readonly string WebhookUrl = Environment.GetEnvironmentVariable("WEBHOOK_URL", EnvironmentVariableTarget.Process); 
        private static readonly string HatenaId = Environment.GetEnvironmentVariable("HATENA_ID", EnvironmentVariableTarget.Process);
        private static readonly string ApiKey = Environment.GetEnvironmentVariable("API_KEY", EnvironmentVariableTarget.Process); 
        private static readonly string EndPoint = Environment.GetEnvironmentVariable("END_POINT", EnvironmentVariableTarget.Process); 
        private static readonly string RandomeMessage = Environment.GetEnvironmentVariable("RANDOM_MESSAGE", EnvironmentVariableTarget.Process);

        static TimerBlogNotification()
        {
            HttpClient = new HttpClient();
            Random = new Random(Seed++);
        }

        [Function("TimerBlogNotification")]
        public static async Task Run([TimerTrigger("0 30 8 * * *")] MyInfo myTimer, FunctionContext context)
        {
            var logger = context.GetLogger("TimerBlogNotification");
            var xml = await GetHatenaBlogXmlText();
            var published = GetLatestBlogReleaseDate(xml);
            string payload = "";

            switch (CompareToBlogDay(DateTime.Parse(published)))
            {
                case -1:
                    payload = SetNoGoodMessage(Random.Next(0, 4));
                    break;
                case int i when i >= 0:
                    payload = SetExcellentMessage();
                    break;
                default:
                    break;
            }

            var res = await HttpClient.PostAsync(WebhookUrl, new StringContent(payload, Encoding.UTF8, "application/json"));

            logger.LogInformation($"C# Timer trigger function executed at: {Now}");
            logger.LogInformation($"Next timer schedule at: {myTimer.ScheduleStatus.Next}");

            res.EnsureSuccessStatusCode();
        }

        public static int CompareToBlogDay(DateTime published)
        {
            return published.CompareTo(Now);
        }

        public static async Task<string> GetHatenaBlogXmlText()
        {
            var byteArray = Encoding.ASCII.GetBytes($"{HatenaId}:{ApiKey}");
            string schema = Convert.ToBase64String(byteArray);

            HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", schema);

            var response = await HttpClient.GetAsync(EndPoint);
            string result = response.Content.ReadAsStringAsync().Result;

            return result;
        }

        public static string GetLatestBlogReleaseDate(string response)
        {
            XmlDocument doc = new();
            doc.LoadXml(response);
            XmlNodeList nodes = doc.GetElementsByTagName("published");
            return nodes[0].InnerText;
        }

        public static string SetNoGoodMessage(int rand)
        {
            string optionMassage = GenerateOptionMessage();
            RandomMessageModel[] model = JsonSerializer.Deserialize<RandomMessageModel[]>(RandomeMessage);        
            return GenerateNotificationMessage(model[rand].Message, model[rand].ImagePath, optionMassage);
        }

        public static string SetExcellentMessage()
        {
            string message = "ブログ更新お疲れさま！\\rストロングゼロを許可しよう！";
            string imagePath = "https://stblognotification.blob.core.windows.net/images/end-toki.jpg";
            return GenerateNotificationMessage(message, imagePath);
        }

        private static string GenerateOptionMessage()
        {
            string optionMessage = "";
            switch(Now.DayOfWeek)
            {
                case DayOfWeek.Monday:
                    optionMessage = "本日は月曜です。\n\n今書いておくと開放されるぞ...";
                    break;
                case DayOfWeek.Tuesday:
                    optionMessage = "週始めの更新が...楽だぜ？";
                    break;
                case DayOfWeek.Wednesday:
                    optionMessage = "あっという間にもう水曜ですっ！";
                    break;
                case DayOfWeek.Thursday:
                    optionMessage = "まだ木曜に書いてないのはヤバない？\n\nところで，Pipelinesは組めたん？";
                    break;
                case DayOfWeek.Friday:
                    optionMessage = "明日休みなのにブログ書いていないなんて...\n\nんなわけないよね？";
                    break;
                case DayOfWeek.Saturday:
                    optionMessage = "平日のサボりを取り返しましょう。";
                    break;
                case DayOfWeek.Sunday:
                    optionMessage = "なるほどっ！？\n\n今週は2記事書くんだね！";
                    break;
                default:
                    break; 
            }
            return optionMessage;
        }

        public static string GenerateNotificationMessage(string message = "", string imagePath = "", string option = "")
        {
            string result = $"{{\"type\":\"message\",\"attachments\":[{{\"contentType\":\"application/vnd.microsoft.card.adaptive\",\"contentUrl\":null,\"content\":{{\"$schema\":\"http://adaptivecards.io/schemas/adaptive-card.json\",\"type\":\"AdaptiveCard\",\"version\":\"1.2\",\"body\":[{{\"type\":\"TextBlock\",\"size\":\"default\",\"weight\":\"bolder\",\"text\":\"本日のブログ更新について（環境変数のテスト実施中）\"}},{{\"type\":\"ColumnSet\",\"columns\":[{{\"type\":\"Column\",\"items\":[{{\"type\":\"Image\",\"url\":\"{imagePath}\",\"size\":\"Large\"}}],\"width\":\"auto\"}},{{\"type\":\"Column\",\"items\":[{{\"type\":\"TextBlock\",\"weight\":\"Bolder\",\"text\":\"メッセージ@ブログ監察BOT \r\",\"wrap\":true}},{{\"type\":\"TextBlock\",\"spacing\":\"None\",\"isSubtle\":true,\"wrap\":true,\"text\":\"{message}\r\r{option}\"}},{{\"type\":\"TextBlock\",\"text\":\"[tanabebe blog](https://tanabebe.hatenablog.com/archive)\",\"wrap\":true}}],\"width\":\"stretch\"}}]}}]}}}}]}}";

            return result;
        }
    }

    public class RandomMessageModel
    {
        public string Message { get; set; }
        public string ImagePath { get; set; }
    }
    public class MyInfo
    {
        public MyScheduleStatus ScheduleStatus { get; set; }

        public bool IsPastDue { get; set; }
    }

    public class MyScheduleStatus
    {
        public DateTime Last { get; set; }

        public DateTime Next { get; set; }

        public DateTime LastUpdated { get; set; }
    }
}
