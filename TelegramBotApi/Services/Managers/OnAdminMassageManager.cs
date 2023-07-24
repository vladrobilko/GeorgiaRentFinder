using Application.Converters;
using Application.Interfaces;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Bot.Types;
using Telegram.Bot;
using Application.Models;
using System.Timers;
using Telegram.Bot.Exceptions;
using System.Text.RegularExpressions;

namespace TelegramBotApi.Services.Managers
{
    public class OnAdminMassageManager
    {
        private static System.Timers.Timer? _timer;

        public static bool IsAutoSendingModeStarted { get; set; }

        protected OnAdminMassageManager() { }

        public static async Task<Message> BotStart(ITelegramBotClient bot, IFlatInfoService informer, Message message, CancellationToken cancel)
        {
            var text = MessageToAdminManager.GetMessageFlatCountInfo(informer.GetCountNotViewedFlats());

            return await SendTextMessageAsync(bot, message, cancel, text);
        }

        public static async Task<Message> GetLastAvailableFlat(ITelegramBotClient bot, IFlatInfoService informer, IFlatPublicationService publisher,
            IConfiguration conf, Message mes, CancellationToken cancel)
        {
            var flat = informer.GetAvailableFlat();

            if (flat == null)
            {
                return await SendTextMessageAsync(bot, mes, cancel, MessageToAdminManager.GetMessageFlatCountInfo(0));
            }

            var channelName = informer.GetIdChannelWithLastCheckDate();

            await SendContentToTelegramWithTranslateText(bot, publisher, mes.Chat.Id, flat, conf, cancel, true);

            return await SendTextMessageAsync(bot, mes, cancel, "Choose", GetKeyboardWithChoose(flat, channelName));
        }

        public static async Task<Message> FindSuitAdjaraFlats(ITelegramBotClient bot, IFlatFindService finder, IFlatInfoService informer,
            IConfiguration conf, Message mes, CancellationToken cancel)
        {
            var countNotViewedFlats = informer.GetCountNotViewedFlats();

            if (countNotViewedFlats != 0)
            {
                var textWithCount = MessageToAdminManager.GetMessageFlatCountInfo(countNotViewedFlats);

                return await SendTextMessageAsync(bot, mes, cancel, textWithCount, new ReplyKeyboardRemove());
            }

            finder.FindAndSaveSuitAdjaraFlats(long.Parse(conf.GetSection("AdjaraChannel")["ChannelId"] ?? throw new NotImplementedException()));

            var text = MessageToAdminManager.GetMessageFlatCountInfo(informer.GetCountNotViewedFlats());

            return await SendTextMessageAsync(bot, mes, cancel, text, new ReplyKeyboardRemove());
        }

        public static async Task<Message> FindSuitImeretiFlats(ITelegramBotClient bot, IFlatFindService finder, IFlatInfoService informer,
            IConfiguration conf, Message mes, CancellationToken cancel)
        {
            var countNotViewedFlats = informer.GetCountNotViewedFlats();

            if (countNotViewedFlats != 0)
            {
                return await bot.SendTextMessageAsync(
                    chatId: mes.Chat.Id,
                    text: MessageToAdminManager.GetMessageFlatCountInfo(countNotViewedFlats),
                    parseMode: ParseMode.Html,
                    replyMarkup: new ReplyKeyboardRemove(),
                    cancellationToken: cancel);
            }

            finder.FindAndSaveSuitImeretiFlats(long.Parse(conf.GetSection("ImeretiChannel")["ChannelId"] ?? throw new NotImplementedException()));

            var text = MessageToAdminManager.GetMessageFlatCountInfo(informer.GetCountNotViewedFlats());
            return await SendTextMessageAsync(bot, mes, cancel, text, new ReplyKeyboardRemove());
        }

        public static async Task<Message> AutoFlatSendingEveryHour(ITelegramBotClient bot, IFlatFindService finder, IFlatInfoService informer,
            IFlatPublicationService publisher, IConfiguration conf, Message mes, CancellationToken cancel)
        {
            if (informer.GetCountNotViewedFlats() != 0 || IsAutoSendingModeStarted)
            {
                var text = MessageToAdminManager.GetMessageForTimerStopIfException(informer.GetCountNotViewedFlats(), IsAutoSendingModeStarted);

                await SendTextMessageAsync(bot, mes, cancel, text);

                _timer?.Stop();

                throw new NotSupportedException();
            }

            await AutoFlatSendingWithoutChecking(bot, finder, informer, publisher, conf, mes, cancel);
            var twoHoursInMilliseconds = 60 * 60 * 1000;
            _timer = new System.Timers.Timer(twoHoursInMilliseconds);
            _timer.Elapsed += async (source, e) =>
                await AutoFlatSendingWithoutCheckingOnTimedEvent(source, e, bot, informer, finder, publisher, conf, mes, cancel);
            _timer.Start();
            IsAutoSendingModeStarted = true;

            return await SendTextMessageAsync(bot, mes, cancel, MessageToAdminManager.GetMessageForStartAutoFlatSendingEveryHour);
        }

        private static async Task AutoFlatSendingWithoutCheckingOnTimedEvent(object source, ElapsedEventArgs e, ITelegramBotClient bot, IFlatInfoService informer,
            IFlatFindService finder, IFlatPublicationService publisher, IConfiguration conf, Message mes, CancellationToken cancel)
        {
            if (informer.GetCountNotViewedFlats() != 0 || !IsAutoSendingModeStarted)
            {
                var text = MessageToAdminManager.GetMessageForTimerStopIfException(informer.GetCountNotViewedFlats(), IsAutoSendingModeStarted);

                await SendTextMessageAsync(bot, mes, cancel, text);

                _timer?.Stop();

                throw new NotSupportedException();
            }

            await AutoFlatSendingWithoutChecking(bot, finder, informer, publisher, conf, mes, cancel);
        }

        private static async Task AutoFlatSendingWithoutChecking(ITelegramBotClient bot, IFlatFindService finder, IFlatInfoService informer,
            IFlatPublicationService publisher, IConfiguration conf, Message mes, CancellationToken cancel)
        {
            long countProcessedFlats = 0;

            finder.FindAndSaveSuitAdjaraFlats(long.Parse(conf.GetSection("AdjaraChannel")["ChannelId"] ??
                                                              throw new NotImplementedException()));
            var countNotViewedFlats = informer.GetCountNotViewedFlats();
            countProcessedFlats += countNotViewedFlats;
            await SendFlatsWhileExistAvailableFlatWithDelay(countNotViewedFlats, informer, publisher, bot, conf,
                cancel);

            finder.FindAndSaveSuitImeretiFlats(long.Parse(conf.GetSection("ImeretiChannel")["ChannelId"] ??
                                                               throw new NotImplementedException()));
            countNotViewedFlats = informer.GetCountNotViewedFlats();
            countProcessedFlats += countNotViewedFlats;
            await SendFlatsWhileExistAvailableFlatWithDelay(countNotViewedFlats, informer, publisher, bot, conf,
                cancel);

            await SendTextMessageAsync(bot, mes, cancel, MessageToAdminManager.GetMessageCountOfProcessedFlats(countProcessedFlats));
        }

        public static async Task<Message> OnTextResponse(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
        {
            return await SendTextMessageAsync(botClient, message, cancellationToken, MessageToAdminManager.GetMessageForAfterOnlyTextSending, new ReplyKeyboardRemove());
        }

        public static async Task SendContentToTelegramWithTranslateText(ITelegramBotClient bot, IFlatPublicationService publisher, ChatId chatId,
            FlatInfoClientModel flat, IConfiguration conf, CancellationToken cancel, bool isForAdmin)
        {
            var photos = new IAlbumInputMedia[flat.LinksOfImages.Count];

            var apiTranslatorToken = conf.GetSection("GoogleTranslatorConfiguration")["Token"] ??
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
                await bot.SendMediaGroupAsync(chatId, photos, cancellationToken: cancel);
            }
            catch (ApiRequestException ex)
            {
                if (int.TryParse(Regex.Match(ex.Message, @"\b(?:[0-9]|10)\b").Value, out int number) && flat.LinksOfImages.Count > 2)
                {
                    flat.LinksOfImages.RemoveAt(number - 1);
                    await SendContentToTelegramWithTranslateText(bot, publisher, chatId, flat, conf, cancel, isForAdmin);
                }
                else
                {
                    await OnSendMediaGroupExceptionResponse(bot, publisher, flat, conf, cancel);
                    throw;
                }
            }
            catch
            {
                await OnSendMediaGroupExceptionResponse(bot, publisher, flat, conf, cancel);
                throw;
            }
        }

        private static async Task OnSendMediaGroupExceptionResponse(ITelegramBotClient bot, IFlatPublicationService publisher,
            FlatInfoClientModel flat, IConfiguration conf, CancellationToken cancel)
        {
            await bot.SendTextMessageAsync(
                chatId: conf.GetSection("BotConfiguration")["BotId"] ?? throw new InvalidOperationException(),
                text: MessageToAdminManager.GetMessageAfterExceptionWithSendMediaGroupAsyncToTelegram(flat.Id),
                parseMode: ParseMode.Html,
                cancellationToken: cancel);

            publisher.AddDatesForTelegramException(flat.Id, DateTime.Now);
        }

        private static async Task SendFlatsWhileExistAvailableFlatWithDelay(long countNotViewedFlats, IFlatInfoService informer,
            IFlatPublicationService publisher, ITelegramBotClient bot, IConfiguration conf, CancellationToken cancel)
        {
            while (countNotViewedFlats != 0)
            {
                var flat = informer.GetAvailableFlat();

                var channelName = informer.GetIdChannelWithLastCheckDate();

                if (flat.LinksOfImages.Count > 2)
                {
                    await Task.Delay(60 * 1000, cancel);
                    await SendContentToTelegramWithTranslateText(bot, publisher, channelName, flat, conf,
                        cancel, false);
                    publisher.AddDateOfTelegramPublication(flat.Id, DateTime.Now);
                }
                else
                {
                    publisher.AddDateOfRefusePublication(flat.Id, DateTime.Now);
                }

                countNotViewedFlats = informer.GetCountNotViewedFlats();
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