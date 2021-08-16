using System;
using System.Text.Json;
using tanabebe.tech.function;

namespace HatebNotification.Service
{
    public static class MessageService
    {
        private static readonly Random Random;
        private static readonly int Seed = Environment.TickCount;
        private static readonly string RandomeMessage = Environment.GetEnvironmentVariable("RANDOM_MESSAGE", EnvironmentVariableTarget.Process);

        static MessageService()
        {
            Random = new Random(Seed++);
        }

        public static string CompareToBlogDay(DateTime published, DateTime now)
        {
            string payload = "";
            switch (published.CompareTo(now))
            {
                case -1:
                    payload = SetNoGoodMessage(Random.Next(0, 4),  now);
                    break;
                case int i when i >= 0:
                    payload = SetExcellentMessage();
                    break;
                default:
                    break;
            }
            return payload;
        }

        public static string SetNoGoodMessage(int rand, DateTime now)
        {
            string optionMassage = GenerateOptionMessage(now);
            RandomMessageModel[] model = JsonSerializer.Deserialize<RandomMessageModel[]>(RandomeMessage);
            return GenerateNotificationMessage(model[rand].Message, model[rand].ImagePath, optionMassage);
        }

        public static string SetExcellentMessage()
        {
            string message = Constants.SuccessMessages.Ok;
            string imagePath = "https://stblognotification.blob.core.windows.net/images/end-toki.jpg";
            return GenerateNotificationMessage(message, imagePath);
        }

        private static string GenerateOptionMessage(DateTime now)
        {
            string optionMessage = "";
            switch (now.DayOfWeek)
            {
                case DayOfWeek.Monday:
                    optionMessage = Constants.DayOfWeekMessages.Monday;
                    break;
                case DayOfWeek.Tuesday:
                    optionMessage = Constants.DayOfWeekMessages.Tuesday;
                    break;
                case DayOfWeek.Wednesday:
                    optionMessage = Constants.DayOfWeekMessages.Wednesday;
                    break;
                case DayOfWeek.Thursday:
                    optionMessage = Constants.DayOfWeekMessages.Thursday;
                    break;
                case DayOfWeek.Friday:
                    optionMessage = Constants.DayOfWeekMessages.Friday;
                    break;
                case DayOfWeek.Saturday:
                    optionMessage = Constants.DayOfWeekMessages.Saturday;
                    break;
                case DayOfWeek.Sunday:
                    optionMessage = Constants.DayOfWeekMessages.Sunday;
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

}
