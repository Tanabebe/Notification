using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Xml;
using HatebNotification;
using HatebNotification.Service;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace tanabebe.tech.function
{
    public static class TimerBlogNotification
    {
        private static readonly DateTime Now = DateTime.Now;

        [Function("TimerBlogNotification")]
        public static async Task Run([TimerTrigger("0 30 8 * * *")] MyInfo myTimer, FunctionContext context)
        {
            var logger = context.GetLogger("TimerBlogNotification");
            var xml = await BlogService.GetHatenaBlogXmlText();
            var published = BlogService.GetLatestBlogReleaseDate(xml);
            string payload = MessageService.CompareToBlogDay(DateTime.Parse(published), Now);
            var res = await BlogService.PostBlogAsync(payload);
            logger.LogInformation($"C# Timer trigger function executed at: {Now}");
            logger.LogInformation($"Next timer schedule at: {myTimer.ScheduleStatus.Next}");
            res.EnsureSuccessStatusCode();
        }
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
