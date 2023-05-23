using Application.Converters;

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

        public static string GetMessageAfterOnlyTextSending()
        {
            return "Sorry, only commands";
        }

        public static string GetStartMessage()
        {
            return "Hello, I'm an admin bot of these channels:" +
                   "\n@AdjaraLowRent" +
                   "\n@ImeretiLowRent" +
                   $"\nMy started time is {DateTime.Now.ToCommonViewString()}" +
                   "\nPress /start if you're an admin";
        }

        public static string GetMessageFlatCountInfo(long countFlats)
        {
            if (countFlats == 0)
            {
                return "<ins><strong>No free flats</strong></ins>" +
                       "\n  _____________" +
                       "\n/ImeretiSearch" +
                       "\n  _____________" +
                       "\n/AdjaraSearch";
            }
            return $"There are <ins><strong>{countFlats} NOT distributed flats.</strong></ins>" +
                   $"\nYou need to do this: {GetLastAvailableFlatLink}";
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
    }
}