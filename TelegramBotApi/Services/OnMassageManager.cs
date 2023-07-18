using Application.Converters;
using Application.Interfaces;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Bot.Types;
using Telegram.Bot;
using Application.Models;
using System.Timers;

namespace TelegramBotApi.Services
{
    public class OnMassageManager
    {
        private static System.Timers.Timer? _timer;

        public static bool IsAutoSendingModeStarted { get; set; }

        protected OnMassageManager() { }

        public static async Task<Message> BotStart(ITelegramBotClient botClient, IFlatFindService flatFindService, IFlatInfoService flatInfoService, Message message, CancellationToken cancellationToken)
        {
            var text = BotMessageManager.GetMessageFlatCountInfo(flatInfoService.GetCountNotViewedFlats());

            return await SendTextMessageAsync(botClient, message, cancellationToken, text);
        }

        public static async Task<Message> GetLastAvailableFlat(ITelegramBotClient botClient, IFlatFindService flatFindService, IFlatInfoService flatInfoService, IFlatPublicationService flatPublicationService,
            IConfiguration configuration, Message message, CancellationToken cancellationToken)
        {
            var flat = flatInfoService.GetAvailableFlat();

            if (flat == null)
            {
                return await SendTextMessageAsync(botClient, message, cancellationToken, BotMessageManager.GetMessageFlatCountInfo(0));
            }

            var channelName = flatInfoService.GetIdChannelWithLastCheckDate();

            await SendContentToTelegramWithTranslateText(botClient, flatPublicationService, message.Chat.Id, flat, configuration, cancellationToken, true);

            return await SendTextMessageAsync(botClient, message, cancellationToken, "Choose", GetKeyboardWithChoose(flat, channelName));
        }

        public static async Task<Message> FindSuitAdjaraFlats(ITelegramBotClient botClient, IFlatFindService flatFindService, IFlatInfoService flatInfoService,
            IConfiguration configuration, Message message, CancellationToken cancellationToken)
        {
            var countNotViewedFlats = flatInfoService.GetCountNotViewedFlats();

            if (countNotViewedFlats != 0)
            {
                var textWithCount = BotMessageManager.GetMessageFlatCountInfo(countNotViewedFlats);

                return await SendTextMessageAsync(botClient, message, cancellationToken, textWithCount, new ReplyKeyboardRemove());
            }

            flatFindService.FindAndSaveSuitAdjaraFlats(long.Parse(configuration.GetSection("AdjaraChannel")["ChannelId"] ?? throw new NotImplementedException()));

            var text = BotMessageManager.GetMessageFlatCountInfo(flatInfoService.GetCountNotViewedFlats());

            return await SendTextMessageAsync(botClient, message, cancellationToken, text, new ReplyKeyboardRemove());
        }

        public static async Task<Message> FindSuitImeretiFlats(ITelegramBotClient botClient, IFlatFindService flatFindService, IFlatInfoService flatInfoService,
            IConfiguration configuration, Message message, CancellationToken cancellationToken)
        {
            var countNotViewedFlats = flatInfoService.GetCountNotViewedFlats();

            if (countNotViewedFlats != 0)
            {
                return await botClient.SendTextMessageAsync(
                    chatId: message.Chat.Id,
                    text: BotMessageManager.GetMessageFlatCountInfo(countNotViewedFlats),
                    parseMode: ParseMode.Html,
                    replyMarkup: new ReplyKeyboardRemove(),
                    cancellationToken: cancellationToken);
            }

            flatFindService.FindAndSaveSuitImeretiFlats(long.Parse(configuration.GetSection("ImeretiChannel")["ChannelId"] ?? throw new NotImplementedException()));

            var text = BotMessageManager.GetMessageFlatCountInfo(flatInfoService.GetCountNotViewedFlats());
            return await SendTextMessageAsync(botClient, message, cancellationToken, text, new ReplyKeyboardRemove());
        }

        public static async Task<Message> AutoFlatSendingEveryHour(ITelegramBotClient botClient, IFlatFindService flatFindService, IFlatInfoService flatInfoService,
            IFlatPublicationService flatPublicationService, IConfiguration configuration, Message message, CancellationToken cancellationToken)
        {
            if (flatInfoService.GetCountNotViewedFlats() != 0 || IsAutoSendingModeStarted)
            {
                var text = BotMessageManager.GetMessageForTimerStopIfException(flatInfoService.GetCountNotViewedFlats(), IsAutoSendingModeStarted);

                await SendTextMessageAsync(botClient, message, cancellationToken, text);

                _timer?.Stop();

                throw new NotSupportedException();
            }

            await AutoFlatSendingWithoutChecking(botClient, flatFindService, flatInfoService, flatPublicationService, configuration, message, cancellationToken);
            var twoHoursInMilliseconds = 60 * 60 * 1000;
            _timer = new System.Timers.Timer(twoHoursInMilliseconds);
            _timer.Elapsed += async (source, e) =>
                await AutoFlatSendingWithoutCheckingOnTimedEvent(source, e, botClient, flatInfoService, flatFindService, flatPublicationService, configuration, message, cancellationToken);
            _timer.Start();
            IsAutoSendingModeStarted = true;

            return await SendTextMessageAsync(botClient, message, cancellationToken, BotMessageManager.GetMessageForStartAutoFlatSendingEveryTwoHour);
        }

        private static async Task AutoFlatSendingWithoutCheckingOnTimedEvent(object source, ElapsedEventArgs e, ITelegramBotClient botClient, IFlatInfoService flatInfoService,
            IFlatFindService flatFindService, IFlatPublicationService flatPublicationService, IConfiguration configuration, Message message, CancellationToken cancellationToken)
        {
            if (flatInfoService.GetCountNotViewedFlats() != 0 || !IsAutoSendingModeStarted)
            {
                var text = BotMessageManager.GetMessageForTimerStopIfException(flatInfoService.GetCountNotViewedFlats(), IsAutoSendingModeStarted);

                await SendTextMessageAsync(botClient, message, cancellationToken, text);

                _timer?.Stop();

                throw new NotSupportedException();
            }

            await AutoFlatSendingWithoutChecking(botClient, flatFindService, flatInfoService, flatPublicationService, configuration, message, cancellationToken);
        }

        private static async Task AutoFlatSendingWithoutChecking(ITelegramBotClient botClient, IFlatFindService flatFindService, IFlatInfoService flatInfoService,
            IFlatPublicationService flatPublicationService, IConfiguration configuration, Message message, CancellationToken cancellationToken)
        {
            long countProcessedFlats = 0;

            flatFindService.FindAndSaveSuitAdjaraFlats(long.Parse(configuration.GetSection("AdjaraChannel")["ChannelId"] ??
                                                              throw new NotImplementedException()));
            var countNotViewedFlats = flatInfoService.GetCountNotViewedFlats();
            countProcessedFlats += countNotViewedFlats;
            await SendFlatsWhileExistAvailableFlatWithDelay(countNotViewedFlats, flatInfoService, flatPublicationService, botClient, configuration,
                cancellationToken);

            flatFindService.FindAndSaveSuitImeretiFlats(long.Parse(configuration.GetSection("ImeretiChannel")["ChannelId"] ??
                                                               throw new NotImplementedException()));
            countNotViewedFlats = flatInfoService.GetCountNotViewedFlats();
            countProcessedFlats += countNotViewedFlats;
            await SendFlatsWhileExistAvailableFlatWithDelay(countNotViewedFlats, flatInfoService, flatPublicationService, botClient, configuration,
                cancellationToken);

            await SendTextMessageAsync(botClient, message, cancellationToken, BotMessageManager.GetMessageCountOfProcessedFlats(countProcessedFlats));
        }

        public static async Task<Message> OnTextResponse(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
        {
            return await SendTextMessageAsync(botClient, message, cancellationToken, BotMessageManager.GetMessageForAfterOnlyTextSending, new ReplyKeyboardRemove());
        }

        public static async Task SendContentToTelegramWithTranslateText(ITelegramBotClient botClient, IFlatPublicationService flatPublicationService, ChatId chatId,
            FlatInfoClientModel flat, IConfiguration configuration, CancellationToken cancellationToken, bool isForAdmin)
        {
            var photos = new IAlbumInputMedia[flat.LinksOfImages.Count];

            var apiTranslatorToken = configuration.GetSection("GoogleTranslatorConfiguration")["Token"] ??
                                     throw new InvalidOperationException();

            for (var i = 0; i < flat.LinksOfImages.Count; i++)
            {
                if (i == 0)
                {
                    photos[i] = new InputMediaPhoto(flat.LinksOfImages[i])
                    {
                        Caption = flat.ToTelegramCaptionWithRussianLanguage(isForAdmin, "ru", apiTranslatorToken),
                        ParseMode = ParseMode.Html
                    };
                }

                else photos[i] = new InputMediaPhoto(flat.LinksOfImages[i]);
            }

            try
            {
                await botClient.SendMediaGroupAsync(chatId, photos, cancellationToken: cancellationToken);
            }
            catch
            {
                await botClient.SendTextMessageAsync(
                    chatId: configuration.GetSection("BotConfiguration")["BotId"] ?? throw new InvalidOperationException(),
                    text: BotMessageManager.GetMessageAfterExceptionWithSendMediaGroupAsyncToTelegram(flat.Id),
                    parseMode: ParseMode.Html,
                    cancellationToken: cancellationToken);

                flatPublicationService.AddDatesForTelegramException(flat.Id, DateTime.Now);
            }
        }

        private static async Task SendFlatsWhileExistAvailableFlatWithDelay(long countNotViewedFlats, IFlatInfoService flatInfoService,
            IFlatPublicationService flatPublicationService, ITelegramBotClient botClient, IConfiguration configuration, CancellationToken cancellationToken)
        {
            while (countNotViewedFlats != 0)
            {
                var flat = flatInfoService.GetAvailableFlat();

                var channelName = flatInfoService.GetIdChannelWithLastCheckDate();

                if (flat.LinksOfImages.Count > 2)
                {
                    await Task.Delay(60 * 1000, cancellationToken);
                    await SendContentToTelegramWithTranslateText(botClient, flatPublicationService, channelName, flat, configuration,
                        cancellationToken, false);
                    flatPublicationService.AddDateOfTelegramPublication(flat.Id, DateTime.Now);
                }
                else
                {
                    flatPublicationService.AddDateOfRefusePublication(flat.Id, DateTime.Now);
                }

                countNotViewedFlats = flatInfoService.GetCountNotViewedFlats();
            }
        }

        private static InlineKeyboardMarkup GetKeyboardWithChoose(FlatInfoClientModel flat, string channelName)
        {
            return new(
                new[]
                {
                    new []
                    {
                        InlineKeyboardButton.WithCallbackData("✅✅Post✅✅",$"post_{flat.Id}_{channelName}"),
                        InlineKeyboardButton.WithCallbackData("❌DON'T post❌",$"no post_{flat.Id}_noChannel"),
                    }
                });
        }

        private static async Task<Message> SendTextMessageAsync(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken, string text)
        {
            return await botClient.SendTextMessageAsync(
                chatId: message.Chat.Id,
                text: text,
                parseMode: ParseMode.Html,
                cancellationToken: cancellationToken);
        }

        private static async Task<Message> SendTextMessageAsync(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken, string text, IReplyMarkup replyMarkup)
        {
            return await botClient.SendTextMessageAsync(
                chatId: message.Chat.Id,
                text: text,
                parseMode: ParseMode.Html,
                cancellationToken: cancellationToken,
                replyMarkup: replyMarkup);
        }
    }
}