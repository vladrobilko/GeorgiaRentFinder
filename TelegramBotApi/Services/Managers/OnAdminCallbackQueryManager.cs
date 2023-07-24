using Application.Interfaces;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Bot.Types;

namespace TelegramBotApi.Services.Managers
{
    public class OnAdminCallbackQueryManager
    {
        protected OnAdminCallbackQueryManager() { }

        public static async Task ChooseFLatPostFromAdmin(CallbackQuery callbackQuery, CancellationToken cancellationToken, ITelegramBotClient botClient,
            IConfiguration configuration, IFlatPublicationService flatPublicationService, IFlatInfoService flatInfoService)
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
                await OnAdminMassageManager.SendContentToTelegramWithTranslateText(botClient, flatPublicationService, channelName, flat, configuration, cancellationToken, false);

                flatPublicationService.AddDateOfTelegramPublication(flat.Id, DateTime.Now);

                textResponseToBot = MessageToAdminManager.GetMessageAfterPost(flatInfoService.GetCountNotViewedFlats());
            }

            else if (infoData == "no post")
            {
                flatPublicationService.AddDateOfRefusePublication(flat.Id, DateTime.Now);

                textResponseToBot = MessageToAdminManager.GetMessageAfterRefusePost(flatInfoService.GetCountNotViewedFlats());
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