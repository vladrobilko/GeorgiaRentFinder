namespace TelegramBotApi.Services.Managers
{
    public static class MessageToUserManager
    {
        public static string GetMessageAdminInfo(string language)
        {
            if (language == "ka")
            {
                return "@sakartvelo_rent <= ადმინისტრატორი" +
                       "\n" +
                       "\n/rent <= ქირავდება" +
                       "\n" +
                       "\n/rentOut <= გაივლის";
            }

            return "@sakartvelo_rent <= администратор" +
                   "\n" +
                   "\n/rent <= арендовать" +
                   "\n" +
                   "\n/rentOut <= сдать";
        }
        public static string GetMessageRentInfo(string language)
        {
            if (language == "ka")
            {
                return "@AdjaraLowRent <= ქირავდება ბათუმი და ქობულეთი" +
                       "\n" +
                       "\n@ImeretiLowRent <= ქირავდება ქუთაისი";
            }

            return "@AdjaraLowRent <= аренда Батуми и Кобулети" +
                   "\n" +
                   "\n@ImeretiLowRent <= аренда Кутаиси";
        }

        public static string GetStartMessage()
        {
            return "Выберите язык / აირჩიეთ ენა:" +
                   "\n";
        }

        public static string GetMessageForAfterOnlyTextSending(string language)
        {
            if (language == "ka")
            {
                return "შეცდომა, არასწორი ბრძანება.";
            }

            return "Ошибка, неверная команда.";
        }

        public static string GetMessageWithCallBackQueryOnChooseLanguage(string language)
        {
            if (language == "ka")
            {
                return "აირჩიეთ ქალაქი";
            }

            return "Выберите город";
        }
        public static string GetMessageWithCallBackQueryOnChooseCity(string language)
        {
            if (language == "ka")// translate it
            {
                return "უძრავი ქონების გაქირავების ბოტი საქართველოში" +
                       "\n" +
                       "\nდააწკაპუნეთ:" +
                       "\n" +
                       "\n/rent <= თუ გსურთ ქონების დაქირავება" +
                       "\n" +
                       "\n/rentOut <= თუ გსურთ თქვენი ქონების გაქირავება" +
                       "\n" +
                       "\n/admin <= ადმინისტრატორთან დასაკავშირებლად";
            }

            return "Бот по аренде недвижимости в Грузии" +
                   "\n" +
                   "\nНажмите:" +
                   "\n" +
                   "\n/rent <= Если вы хотите арендовать недвижимость" +
                   "\n" +
                   "\n/rentOut <= Если вы хотите сдать свою недвижимость" +
                   "\n" +
                   "\n/admin <= Для связи с администратором";
        }
    }
}
