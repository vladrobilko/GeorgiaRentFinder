using Application.Interfaces;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Bot.Types;
using Telegram.Bot;

namespace TelegramBotApi.Services.Managers
{
    public class OnUserCallbackQueryManager
    {
        protected OnUserCallbackQueryManager() { }

        public static async Task ChooseLanguage(CallbackQuery callbackQuery, CancellationToken cancellationToken, ITelegramBotClient botClient,
            IConfiguration configuration, IFlatPublicationService flatPublicationService, IFlatInfoService flatInfoService)
        {
            if (callbackQuery.Data == null || callbackQuery.Message == null) throw new NotImplementedException();

            var callBackInfo = callbackQuery.Data.Split("_");
            
            var googleCodeLanguage = callBackInfo[1];
            var userId = callBackInfo[2];
            string textResponseToBot = "Default";
            var flat = flatInfoService.GetFlatById(long.Parse(googleCodeLanguage));

            if (googleCodeLanguage == "ru")
            {
                textResponseToBot = "Привет";
            }

            else if (googleCodeLanguage == "ka")
            {
                textResponseToBot = "გამარჯობა როგორ ხარ";
            }

            await botClient.SendTextMessageAsync(
                chatId: callbackQuery.Message.Chat.Id,
                text: textResponseToBot,
                parseMode: ParseMode.Html,
                replyMarkup: new InlineKeyboardMarkup(new InlineKeyboardButton[][] { }),
                cancellationToken: cancellationToken);
        }
    }
}