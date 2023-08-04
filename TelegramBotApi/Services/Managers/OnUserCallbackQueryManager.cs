using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Bot.Types;
using Telegram.Bot;

namespace TelegramBotApi.Services.Managers
{
    public class OnUserCallbackQueryManager
    {
        private readonly ITelegramBotClient _bot;

        public OnUserCallbackQueryManager(ITelegramBotClient bot)
        {
            _bot = bot;
        }

        public  async Task ChooseLanguageAndGiveChoiceForCity(CallbackQuery callbackQuery, CancellationToken cancellationToken)
        {
            if (callbackQuery.Data == null || callbackQuery.Message == null || callbackQuery.Message.From == null) throw new NotImplementedException();

            var callBackInfo = callbackQuery.Data.Split("_");
            
            var googleCodeLanguage = callBackInfo[1];
            var userId = callBackInfo[2]; // save info with language here?

            await _bot.DeleteMessageAsync(callbackQuery.Message.Chat.Id, callbackQuery.Message.MessageId, cancellationToken);

            await _bot.SendTextMessageAsync(
                chatId: callbackQuery.Message.Chat.Id,
                text: MessageToUserManager.GetMessageWithCallBackQueryOnChooseLanguage(googleCodeLanguage),
                replyMarkup: GetKeyboardWithCityChoice(callbackQuery.Message, googleCodeLanguage),
                cancellationToken: cancellationToken);
        }

        public  async Task ChooseCityAndGiveChoiceForAction(CallbackQuery callbackQuery, CancellationToken cancellationToken)
        {
            if (callbackQuery.Data == null || callbackQuery.Message == null) throw new NotImplementedException();

            var callBackInfo = callbackQuery.Data.Split("_");

            var city = callBackInfo[1];
            var userId = callBackInfo[2]; // save info about city

            var googleCodeLanguage = "ru"; // get language from db

            await _bot.DeleteMessageAsync(callbackQuery.Message.Chat.Id, callbackQuery.Message.MessageId, cancellationToken);
            
            await _bot.SendTextMessageAsync(
                chatId: callbackQuery.Message.Chat.Id,
                text: MessageToUserManager.GetMessageWithCallBackQueryOnChooseCity(googleCodeLanguage),
                cancellationToken: cancellationToken);
        }

        private  InlineKeyboardMarkup GetKeyboardWithCityChoice(Message message,string language)
        {
            return new(
                new[]
                {
                    new []
                    {
                        InlineKeyboardButton.WithCallbackData(language == "ka" ? "თბილისი" : "Тбилиси",$"cityChoice_Tbilisi_{message.From.Id}")
                    },     
                    new []
                    {
                        InlineKeyboardButton.WithCallbackData(language == "ka" ? "ბათუმი" : "Батуми",$"cityChoice_Batumi_{message.From.Id}")
                    },       
                    new []
                    {
                        InlineKeyboardButton.WithCallbackData(language == "ka" ? "ქუთაისი" : "Кутаиси",$"cityChoice_Kutaisi_{message.From.Id}")
                    },      
                    new []
                    {
                        InlineKeyboardButton.WithCallbackData(language == "ka" ? "რუსთავი" : "Рустави",$"cityChoice_Rustavi_{message.From.Id}")
                    },     
                    new []
                    {
                        InlineKeyboardButton.WithCallbackData(language == "ka" ? "ქობულეთი" : "Кобулети",$"cityChoice_Kobuleti_{message.From.Id}")
                    }
                });
        }
    }
}