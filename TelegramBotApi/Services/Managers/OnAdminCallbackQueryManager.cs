using Application.Interfaces;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Bot.Types;

namespace TelegramBotApi.Services.Managers
{
    public class OnAdminCallbackQueryManager
    {
        private readonly ITelegramBotClient _bot;
        private readonly IFlatPublicationService _publisher;
        private readonly IFlatInfoService _informer;

        private readonly OnAdminMassageManager _onAdminMassageManager;

        public OnAdminCallbackQueryManager(ITelegramBotClient bot, IFlatPublicationService publisher, IFlatInfoService informer, OnAdminMassageManager onAdminMassageManager)
        {
            _bot = bot;
            _publisher = publisher;
            _informer = informer;
            _onAdminMassageManager = onAdminMassageManager;
        }

        public  async Task ChooseFLatPostFromAdmin(CallbackQuery callbackQuery, CancellationToken cancellationToken)
        {
            if (callbackQuery.Data == null || callbackQuery.Message == null) throw new NotImplementedException();

            var callBackInfo = callbackQuery.Data.Split("_");

            var infoData = callBackInfo[0];
            var flatId = callBackInfo[1];
            var channelName = callBackInfo[2];
            string textResponseToBot = "Default";
            var flat = _informer.GetFlatById(long.Parse(flatId));

            if (infoData == "post")
            {
                await _onAdminMassageManager.SendContentToTelegramWithTranslateText(channelName, flat, cancellationToken, false);

                _publisher.AddDateOfTelegramPublication(flat.Id, DateTime.Now);

                textResponseToBot = MessageToAdminManager.GetMessageAfterPost(_informer.GetCountNotViewedFlats());
            }

            else if (infoData == "no post")
            {
                _publisher.AddDateOfRefusePublication(flat.Id, DateTime.Now);

                textResponseToBot = MessageToAdminManager.GetMessageAfterRefusePost(_informer.GetCountNotViewedFlats());
            }

            await _bot.SendTextMessageAsync(
                chatId: callbackQuery.Message.Chat.Id,
                text: textResponseToBot,
                parseMode: ParseMode.Html,
                replyMarkup: new InlineKeyboardMarkup(new InlineKeyboardButton[][] { }),
                cancellationToken: cancellationToken);
        }
    }
}