using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace HatebNotification.Service
{
    public static class BlogService
    {
        private static readonly HttpClient HttpClient = new();
        private static readonly string WebhookUrl = Environment.GetEnvironmentVariable("WEBHOOK_URL", EnvironmentVariableTarget.Process);
        private static readonly string HatenaId = Environment.GetEnvironmentVariable("HATENA_ID", EnvironmentVariableTarget.Process);
        private static readonly string ApiKey = Environment.GetEnvironmentVariable("API_KEY", EnvironmentVariableTarget.Process);
        private static readonly string EndPoint = Environment.GetEnvironmentVariable("END_POINT", EnvironmentVariableTarget.Process);


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

        public static async Task<HttpResponseMessage> PostBlogAsync(string payload)
        {
            return await HttpClient.PostAsync(WebhookUrl, new StringContent(payload, Encoding.UTF8, "application/json"));
        }   
    }
}
