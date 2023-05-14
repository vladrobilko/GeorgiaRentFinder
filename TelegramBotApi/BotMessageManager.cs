using WebScraper.Converters;

namespace TelegramBotApi
{
    public static class BotMessageManager
    {
        public const string Usage = "Usage:" +
                                    "\n  _____________" +
                                    "\n/LookFlat" +
                                    "\n  _____________" +
                                    "\n/ImeretiSearch" +
                                    "\n  _____________" +
                                    "\n/AdjaraSearch";

        private const string GetLastAvailableFlatLink = "/LookFlat";

        public static string GetUsageWithTimeNow()
        {
            return "______________________________________" +
                   $"\nThe bot started at {DateTime.Now.ToCommonViewString()}" +
                   $"\n{Usage}";
        }

        public static string GetMessageIfNoFreeFlats()
        {
            return "<ins><strong>No free flats</strong></ins>" +
                    "\n  _____________" +
                    "\n/ImeretiSearch" +
                    "\n  _____________" +
                    "\n/AdjaraSearch";
        }

        public static string GetMessageAfterPost(long flatsKeep)
        {
            if (flatsKeep > 0)
            {
                return $"<ins><strong>The post has been sent!</strong></ins>" +
                       $"\nThere are <ins><strong>{flatsKeep} NOT distributed flats.</strong></ins>" +
                       $"\nYou need to do this: {GetLastAvailableFlatLink}";
            }
            return $"<ins><strong>The post has been sent!</strong></ins>" +
                   $"\n<ins><strong>There are no free flats</strong></ins>" +
                   $"\n{Usage}";
        }

        public static string GetMessageAfterRefusePost(long flatsKeep)
        {
            if (flatsKeep > 0)
            {
                return $"<ins><strong>The post has NOT been sent!</strong></ins>" +
                       $"\nThere are <ins><strong>{flatsKeep} NOT distributed flats.</strong></ins>" +
                       $"\nYou need to do this: {GetLastAvailableFlatLink}";
            }
            return $"<ins><strong>The post has NOT been sent!</strong></ins>" +
                   $"\n<ins><strong>There are no free flats</strong></ins>" +
                   $"\n{Usage}";
        }

        public static string GetMessageWithCountNotViewedFlats(long countNotViewedFlats)
        {
            return $"There are <ins><strong>{countNotViewedFlats} NOT distributed flats.</strong></ins>" +
                   $"\nYou need to do this: {GetLastAvailableFlatLink}";
        }
        public static string GetMessageWithCountFoundedFlats(long countFoundedFlats)
        {
            return $"There are <ins><strong>{countFoundedFlats} NOT distributed flats.</strong></ins>" +
                   $"\nYou need to do this: {GetLastAvailableFlatLink}";
        }
    }
}