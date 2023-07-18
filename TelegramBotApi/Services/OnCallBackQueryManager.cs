using Application.Interfaces;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Bot.Types;

namespace TelegramBotApi.Services
{
    public class OnCallbackQueryManager
    {
        protected OnCallbackQueryManager() { }

        public static async Task ChoosePostingFromAdmin(CallbackQuery callbackQuery, CancellationToken cancellationToken, ITelegramBotClient botClient,
            IConfiguration configuration, IFlatFindService findService, IFlatPublicationService flatPublicationService, IFlatInfoService flatInfoService)
        {
            if (callbackQuery.Data == null || callbackQuery.Message == null) throw new NotImplementedException();

            var callBackInfo = callbackQuery.Data.Split("_");

            var infoData = callBackInfo[0];
            var flatId = callBackInfo[1];
            var channelName = callBackInfo[2];
            string textResponseToBot = "Default";
            var flat = flatInfoService.GetFlatById(long.Parse(flatId));

            if (infoData == "post")
            {
                await OnMassageManager.SendContentToTelegramWithTranslateText(botClient, flatPublicationService, channelName, flat, configuration, cancellationToken, false);

                flatPublicationService.AddDateOfTelegramPublication(flat.Id, DateTime.Now);

                textResponseToBot = BotMessageManager.GetMessageAfterPost(flatInfoService.GetCountNotViewedFlats());
            }

            else if (infoData == "no post")
            {
                flatPublicationService.AddDateOfRefusePublication(flat.Id, DateTime.Now);

                textResponseToBot = BotMessageManager.GetMessageAfterRefusePost(flatInfoService.GetCountNotViewedFlats());
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