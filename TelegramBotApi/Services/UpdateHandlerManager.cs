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
    public class UpdateHandlerManager
    {
        private static System.Timers.Timer _timer;

        public static bool IsAutoSendingModeStarted { get; set; } = false;

        protected UpdateHandlerManager() { }

        public static async Task<Message> BotStart(ITelegramBotClient botClient, IFlatService flatService,
            IConfiguration configuration, Message message, CancellationToken cancellationToken)
        {
            return await botClient.SendTextMessageAsync(
                chatId: message.Chat.Id,
                text: BotMessageManager.GetMessageFlatCountInfo(flatService.GetCountNotViewedFlats()),
                parseMode: ParseMode.Html,
                cancellationToken: cancellationToken);
        }

        public static async Task<Message> GetLastAvailableFlat(ITelegramBotClient botClient, IFlatService flatService,
            IConfiguration configuration, Message message, CancellationToken cancellationToken)
        {
            var flat = flatService.GetAvailableFlat();

            if (flat == null)
            {
                return await botClient.SendTextMessageAsync(
                    chatId: message.Chat.Id,
                    text: BotMessageManager.GetMessageFlatCountInfo(0),
                    parseMode: ParseMode.Html,
                    cancellationToken: cancellationToken);
            }

            var channelName = flatService.GetIdChannelWithLastCheckDate();

            await SendContentToTelegramWithTranslateText(botClient, flatService, message.Chat.Id, flat, configuration, cancellationToken, true);

            return await botClient.SendTextMessageAsync(
                chatId: message.Chat.Id,
                text: "Choose",
                replyMarkup: GetKeyboardWithChoose(flat, channelName),
                cancellationToken: cancellationToken);
        }

        public static async Task<Message> FindSuitAdjaraFlats(ITelegramBotClient botClient, IFlatService flatService,
            IConfiguration configuration, Message message, CancellationToken cancellationToken)
        {
            var countNotViewedFlats = flatService.GetCountNotViewedFlats();

            if (countNotViewedFlats != 0)
            {
                return await botClient.SendTextMessageAsync(
                chatId: message.Chat.Id,
                text: BotMessageManager.GetMessageFlatCountInfo(countNotViewedFlats),
                parseMode: ParseMode.Html,
                replyMarkup: new ReplyKeyboardRemove(),
                    cancellationToken: cancellationToken);
            }

            flatService.FindAndSaveSuitAdjaraFlats(long.Parse(configuration.GetSection("AdjaraChannel")["ChannelId"] ?? throw new NotImplementedException()));

            return await botClient.SendTextMessageAsync(
                chatId: message.Chat.Id,
                text: BotMessageManager.GetMessageFlatCountInfo(flatService.GetCountNotViewedFlats()),
                parseMode: ParseMode.Html,
                replyMarkup: new ReplyKeyboardRemove(),
                cancellationToken: cancellationToken);
        }

        public static async Task<Message> FindSuitImeretiFlats(ITelegramBotClient botClient, IFlatService flatService,
            IConfiguration configuration, Message message, CancellationToken cancellationToken)
        {
            var countNotViewedFlats = flatService.GetCountNotViewedFlats();

            if (countNotViewedFlats != 0)
            {
                return await botClient.SendTextMessageAsync(
                    chatId: message.Chat.Id,
                    text: BotMessageManager.GetMessageFlatCountInfo(countNotViewedFlats),
                    parseMode: ParseMode.Html,
                    replyMarkup: new ReplyKeyboardRemove(),
                    cancellationToken: cancellationToken);
            }

            flatService.FindAndSaveSuitImeretiFlats(long.Parse(configuration.GetSection("ImeretiChannel")["ChannelId"] ?? throw new NotImplementedException()));

            return await botClient.SendTextMessageAsync(
                chatId: message.Chat.Id,
                text: BotMessageManager.GetMessageFlatCountInfo(flatService.GetCountNotViewedFlats()),
                parseMode: ParseMode.Html,
                replyMarkup: new ReplyKeyboardRemove(),
                cancellationToken: cancellationToken);
        }

        public static async Task<Message> AutoFlatSendingEveryTwoHours(ITelegramBotClient botClient, IFlatService flatService,
            IConfiguration configuration, Message message, CancellationToken cancellationToken)
        {
            if (flatService.GetCountNotViewedFlats() != 0 || IsAutoSendingModeStarted)
            {
                await botClient.SendTextMessageAsync(
                    chatId: message.Chat.Id,
                    text: BotMessageManager.GetMessageForTimerStopIfException(flatService.GetCountNotViewedFlats(), IsAutoSendingModeStarted),
                    parseMode: ParseMode.Html,
                    cancellationToken: cancellationToken);

                _timer.Stop();

                throw new NotSupportedException();
            }

            await AutoFlatSendingWithoutChecking(botClient, flatService, configuration, message, cancellationToken);
            var twoHoursInMilliseconds = 2 * 60 * 60 * 1000;
            _timer = new System.Timers.Timer(twoHoursInMilliseconds);
            _timer.Elapsed += async (source, e) => await AutoFlatSendingWithoutCheckingOnTimedEvent(source, e, botClient, flatService, configuration, message, cancellationToken);
            _timer.Start();
            IsAutoSendingModeStarted = true;

            return await botClient.SendTextMessageAsync(
                chatId: message.Chat.Id, 
                text: BotMessageManager.GetMessageForStartAutoFlatSendingEveryTwoHour,
                parseMode: ParseMode.Html,
                cancellationToken: cancellationToken);
        }

        private static async Task AutoFlatSendingWithoutCheckingOnTimedEvent(object source, ElapsedEventArgs e, ITelegramBotClient botClient, IFlatService flatService,
            IConfiguration configuration, Message message, CancellationToken cancellationToken)
        {
            if (flatService.GetCountNotViewedFlats() != 0 || !IsAutoSendingModeStarted)
            {
                await botClient.SendTextMessageAsync(
                    chatId: message.Chat.Id,
                    text: BotMessageManager.GetMessageForTimerStopIfException(flatService.GetCountNotViewedFlats(), IsAutoSendingModeStarted),
                    parseMode: ParseMode.Html,
                    cancellationToken: cancellationToken);

                _timer.Stop();

                throw new NotSupportedException();
            }

            await AutoFlatSendingWithoutChecking(botClient, flatService, configuration, message, cancellationToken);
        }

        private static async Task AutoFlatSendingWithoutChecking(ITelegramBotClient botClient, IFlatService flatService,
            IConfiguration configuration, Message message, CancellationToken cancellationToken)
        {
            long countProcessedFlats = 0;

            flatService.FindAndSaveSuitAdjaraFlats(long.Parse(configuration.GetSection("AdjaraChannel")["ChannelId"] ??
                                                              throw new NotImplementedException()));
            var countNotViewedFlats = flatService.GetCountNotViewedFlats();
            countProcessedFlats += countNotViewedFlats;
            await SendFlatsWhileExistAvailableFlat(countNotViewedFlats, flatService, botClient, configuration,
                cancellationToken);

            flatService.FindAndSaveSuitImeretiFlats(long.Parse(configuration.GetSection("ImeretiChannel")["ChannelId"] ??
                                                               throw new NotImplementedException()));
            countNotViewedFlats = flatService.GetCountNotViewedFlats();
            countProcessedFlats += countNotViewedFlats;
            await SendFlatsWhileExistAvailableFlat(countNotViewedFlats, flatService, botClient, configuration,
                cancellationToken);

            await botClient.SendTextMessageAsync(
                chatId: message.Chat.Id,
                text: BotMessageManager.GetMessageCountOfProcessedFlats(countProcessedFlats),
                parseMode: ParseMode.Html,
                cancellationToken: cancellationToken);
        }

        public static async Task<Message> OnTextResponse(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
        {
            return await botClient.SendTextMessageAsync(
                chatId: message.Chat.Id,
                text: BotMessageManager.GetMessageForAfterOnlyTextSending,
                replyMarkup: new ReplyKeyboardRemove(),
                cancellationToken: cancellationToken);
        }

        public static async Task SendContentToTelegramWithTranslateText(ITelegramBotClient botClient, IFlatService flatService, ChatId chatId,
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

                flatService.AddDatesForTelegramException(flat.Id, DateTime.Now);
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

        private static async Task SendFlatsWhileExistAvailableFlat(long countNotViewedFlats, IFlatService flatService,
            ITelegramBotClient botClient, IConfiguration configuration, CancellationToken cancellationToken)
        {
            while (countNotViewedFlats != 0)
            {
                var flat = flatService.GetAvailableFlat();

                var channelName = flatService.GetIdChannelWithLastCheckDate();

                if (flat.LinksOfImages.Count > 2)
                {
                    await Task.Delay(60 * 1000, cancellationToken);
                    await SendContentToTelegramWithTranslateText(botClient, flatService, channelName, flat, configuration,
                        cancellationToken, false);
                    flatService.AddDateOfTelegramPublication(flat.Id, DateTime.Now);
                }
                else
                {
                    flatService.AddDateOfRefusePublication(flat.Id, DateTime.Now);
                }

                countNotViewedFlats = flatService.GetCountNotViewedFlats();
            }
        }
    }
}