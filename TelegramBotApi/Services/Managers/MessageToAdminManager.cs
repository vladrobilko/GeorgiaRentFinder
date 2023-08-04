using Application.Converters;

namespace TelegramBotApi.Services.Managers
{
    public static class MessageToAdminManager
    {
        public const string Usage = "Usage:" +
                                    "\n  _____________" +
                                    "\n/LookFlat" +
                                    "\n  _____________" +
                                    "\n/ImeretiSearch" +
                                    "\n  _____________" +
                                    "\n/AdjaraSearch" +
                                    "\n  _____________" +
                                    "\n/TbilisiRustaviSearch" +
                                    "\n  _____________" +
                                    "\n/AutoFlatSendingEveryHour";

        public const string GetMessageForStartAutoFlatSendingEveryHour = "Program is working in auto mode";

        public const string GetMessageForAfterOnlyTextSending = "Sorry, only commands";

        public const string GetMessageForGetLastAvailableFlatLink = "/LookFlat";

        public static string GetMessageForTimerStopIfException(long notViewedFlatsCount, bool isTimerStart)
        {
            return $"There is a mistake with timer for these reasons: " +
                   $"\ncount no viewed flats: {notViewedFlatsCount} or timer is started equal {isTimerStart}" +
                   $"\nTimer will be stopped!!!";
        }

        public static string GetMessageCountOfProcessedFlats(long count)
        {
            return $"{count} flats were processed";
        }

        public static string GetMessageAfterExceptionWithSendMediaGroupAsyncToTelegram(long flatId)
        {
            return $"You have a problem. <ins><strong>Flat id is: {flatId}</strong></ins>";
        }

        public static string GetStartMessage()
        {
            return "Hello, I'm a bot of these channels:" +
                   "\n@AdjaraLowRent" +
                   "\n@ImeretiLowRent" +
                   "\n@TbilisiRustaviLowRent" +
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
                       "\n/AdjaraSearch" +
                       "\n  _____________" +
                       "\n/TbilisiRustaviSearch" +
                       "\n  _____________" +
                       "\n/AutoFlatSendingEveryHour";
            }
            return $"There are <ins><strong>{countFlats} NOT distributed flats.</strong></ins>" +
                   $"\nYou need to do this: {GetMessageForGetLastAvailableFlatLink}";
        }

        public static string GetMessageAfterPost(long flatsKeep)
        {
            if (flatsKeep > 0)
            {
                return $"<ins><strong>The post has been sent!</strong></ins>" +
                       $"\nThere are <ins><strong>{flatsKeep} NOT distributed flats.</strong></ins>" +
                       $"\nYou need to do this: {GetMessageForGetLastAvailableFlatLink}";
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
                       $"\nYou need to do this: {GetMessageForGetLastAvailableFlatLink}";
            }
            return $"<ins><strong>The post has NOT been sent!</strong></ins>" +
                   $"\n<ins><strong>There are no free flats</strong></ins>" +
                   $"\n{Usage}";
        }
    }
}