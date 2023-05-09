using WebScraper.Converters;

namespace TelegramBotApi
{
    public static class BotMessageManager
    {
        public const string Usage = "Usage:\n"
                              + "/FindSuitAdjaraFlats\n" +
                              "/GetLastAvailableAdjaraFlat";
        public static string GetUsageWithTimeNow()
        {
            return "______________________________________\n" +
                   $"The bot started at {DateTime.Now.ToCommonViewString()}\n"
                   + Usage;
        }

        public static string GetUsageWithWithNoFreeMessage()
        {
            return $"No free flats\n\n{Usage}";
        }

        public static string GetMessageAfterPost()
        {
            return $"<ins><strong>The post has been sent!</strong></ins>\n{Usage}";
        }

        public static string GetMessageAfterRefusePost()
        {
            return $"<ins><strong>The post has NOT been sent!</strong></ins>\n{Usage}";
        }
    }
}