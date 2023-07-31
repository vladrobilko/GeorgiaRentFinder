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
        private readonly ITelegramBotClient _bot;
        private readonly IConfiguration _conf;
        private readonly IFlatFindService _finder;
        private readonly IFlatPublicationService _publisher;
        private readonly IFlatInfoService _informer;
        private readonly IServiceProvider _provider;

        private static System.Timers.Timer? _timer;

        public static bool IsAutoSendingModeStarted { get; set; }

        public OnAdminMassageManager(ITelegramBotClient bot, IConfiguration conf, IFlatFindService finder,
            IFlatPublicationService publisher, IFlatInfoService informer, IServiceProvider provider)
        {
            _bot = bot;
            _conf = conf;
            _finder = finder;
            _publisher = publisher;
            _informer = informer;
            _provider = provider;
        }

        public async Task<Message> BotStart(Message message, CancellationToken cancel)
        {
            var text = MessageToAdminManager.GetMessageFlatCountInfo(_informer.GetCountNotViewedFlats());

            return await SendTextMessageAsync(message, cancel, text);
        }

        public async Task<Message> GetLastAvailableFlat(Message mes, CancellationToken cancel)
        {
            var flat = _informer.GetAvailableFlat();

            if (flat == null)
            {
                return await SendTextMessageAsync(mes, cancel, MessageToAdminManager.GetMessageFlatCountInfo(0));
            }

            var channelName = _informer.GetIdChannelWithLastCheckDate();

            await SendContentToTelegramWithTranslateText(mes.Chat.Id, flat, cancel, true);

            return await SendTextMessageAsync(mes, cancel, "Choose", GetKeyboardWithChoose(flat, channelName));
        }

        public async Task<Message> FindSuitAdjaraFlats(Message mes, CancellationToken cancel)
        {
            var countNotViewedFlats = _informer.GetCountNotViewedFlats();

            if (countNotViewedFlats != 0)
            {
                var textWithCount = MessageToAdminManager.GetMessageFlatCountInfo(countNotViewedFlats);

                return await SendTextMessageAsync(mes, cancel, textWithCount, new ReplyKeyboardRemove());
            }

            _finder.FindAndSaveSuitAdjaraFlats(long.Parse(_conf.GetSection("AdjaraChannel")["ChannelId"] ?? throw new NotImplementedException()));

            var text = MessageToAdminManager.GetMessageFlatCountInfo(_informer.GetCountNotViewedFlats());

            return await SendTextMessageAsync(mes, cancel, text, new ReplyKeyboardRemove());
        }

        public async Task<Message> FindSuitImeretiFlats(Message mes, CancellationToken cancel)
        {
            var countNotViewedFlats = _informer.GetCountNotViewedFlats();

            if (countNotViewedFlats != 0)
            {
                return await _bot.SendTextMessageAsync(
                    chatId: mes.Chat.Id,
                    text: MessageToAdminManager.GetMessageFlatCountInfo(countNotViewedFlats),
                    parseMode: ParseMode.Html,
                    replyMarkup: new ReplyKeyboardRemove(),
                    cancellationToken: cancel);
            }

            _finder.FindAndSaveSuitImeretiFlats(long.Parse(_conf.GetSection("ImeretiChannel")["ChannelId"] ?? throw new NotImplementedException()));

            var text = MessageToAdminManager.GetMessageFlatCountInfo(_informer.GetCountNotViewedFlats());
            return await SendTextMessageAsync(mes, cancel, text, new ReplyKeyboardRemove());
        }

        public async Task<Message> AutoFlatSendingEveryHour(Message mes, CancellationToken cancel)
        {
            if (_informer.GetCountNotViewedFlats() != 0 || IsAutoSendingModeStarted)
            {
                var text = MessageToAdminManager.GetMessageForTimerStopIfException(_informer.GetCountNotViewedFlats(), IsAutoSendingModeStarted);

                await SendTextMessageAsync(mes, cancel, text);

                _timer?.Stop();

                throw new NotSupportedException();
            }

            await AutoFlatSendingWithoutChecking(mes, cancel);

            var twoHoursInMilliseconds = 60 * 60 * 1000;
            _timer = new System.Timers.Timer(twoHoursInMilliseconds);
            _timer.Elapsed += async (source, e) =>
                await AutoFlatSendingWithoutCheckingOnTimedEvent(source, e, mes, cancel);
            _timer.Start();
            IsAutoSendingModeStarted = true;

            return await SendTextMessageAsync(mes, cancel, MessageToAdminManager.GetMessageForStartAutoFlatSendingEveryHour);
        }

        private async Task AutoFlatSendingWithoutCheckingOnTimedEvent(object source, ElapsedEventArgs e, Message mes, CancellationToken cancel)// here service
        {
            using (var scope = _provider.CreateScope())
            {
                var bot = scope.ServiceProvider.GetRequiredService<ITelegramBotClient>();
                var informer = scope.ServiceProvider.GetRequiredService<IFlatInfoService>();
                var finder = scope.ServiceProvider.GetRequiredService<IFlatFindService>();
                var conf = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                var publisher = scope.ServiceProvider.GetRequiredService<IFlatPublicationService>();

                if (informer.GetCountNotViewedFlats() != 0 || !IsAutoSendingModeStarted)
                {
                    var text = MessageToAdminManager.GetMessageForTimerStopIfException(informer.GetCountNotViewedFlats(), IsAutoSendingModeStarted);

                    await SendTextMessageAsync(bot, mes, cancel, text);

                    _timer?.Stop();

                    throw new NotSupportedException();
                }

                long countProcessedFlats = 0;

                finder.FindAndSaveSuitAdjaraFlats(long.Parse(conf.GetSection("AdjaraChannel")["ChannelId"] ??
                                                             throw new NotImplementedException()));
                var countNotViewedFlats = informer.GetCountNotViewedFlats();
                countProcessedFlats += countNotViewedFlats;
                await SendFlatsWhileExistAvailableFlatWithDelay(conf, bot, informer, publisher, countNotViewedFlats, cancel);

                finder.FindAndSaveSuitImeretiFlats(long.Parse(conf.GetSection("ImeretiChannel")["ChannelId"] ??
                                                              throw new NotImplementedException()));
                countNotViewedFlats = informer.GetCountNotViewedFlats();
                countProcessedFlats += countNotViewedFlats;
                await SendFlatsWhileExistAvailableFlatWithDelay(conf, bot, informer, publisher, countNotViewedFlats, cancel);

                await SendTextMessageAsync(bot, mes, cancel, MessageToAdminManager.GetMessageCountOfProcessedFlats(countProcessedFlats));
            }
        }

        private async Task AutoFlatSendingWithoutChecking(Message mes, CancellationToken cancel)
        {
            long countProcessedFlats = 0;

            _finder.FindAndSaveSuitAdjaraFlats(long.Parse(_conf.GetSection("AdjaraChannel")["ChannelId"] ??
                                                         throw new NotImplementedException()));
            var countNotViewedFlats = _informer.GetCountNotViewedFlats();
            countProcessedFlats += countNotViewedFlats;
            await SendFlatsWhileExistAvailableFlatWithDelay(countNotViewedFlats, cancel);

            _finder.FindAndSaveSuitImeretiFlats(long.Parse(_conf.GetSection("ImeretiChannel")["ChannelId"] ??
                                                           throw new NotImplementedException()));
            countNotViewedFlats = _informer.GetCountNotViewedFlats();
            countProcessedFlats += countNotViewedFlats;
            await SendFlatsWhileExistAvailableFlatWithDelay(countNotViewedFlats, cancel);

            await SendTextMessageAsync(mes, cancel, MessageToAdminManager.GetMessageCountOfProcessedFlats(countProcessedFlats));
        }

        public async Task<Message> OnTextResponse(Message message, CancellationToken cancellationToken)
        {
            return await SendTextMessageAsync(message, cancellationToken, MessageToAdminManager.GetMessageForAfterOnlyTextSending, new ReplyKeyboardRemove());
        }

        public async Task SendContentToTelegramWithTranslateText(ChatId chatId, FlatInfoClientModel flat, CancellationToken cancel, bool isForAdmin)
        {
            var photos = new IAlbumInputMedia[flat.LinksOfImages.Count];

            var apiTranslatorToken = _conf.GetSection("GoogleTranslatorConfiguration")["Token"] ??
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
                await _bot.SendMediaGroupAsync(chatId, photos, cancellationToken: cancel);
            }
            catch (ApiRequestException ex)
            {
                if (int.TryParse(Regex.Match(ex.Message, @"\b(?:[0-9]|10)\b").Value, out int number) && flat.LinksOfImages.Count > 2)
                {
                    flat.LinksOfImages.RemoveAt(number - 1);
                    await SendContentToTelegramWithTranslateText(chatId, flat, cancel, isForAdmin);
                }
                else
                {
                    await OnSendMediaGroupExceptionResponse(flat, cancel);
                }
            }
            catch
            {
                await OnSendMediaGroupExceptionResponse(flat, cancel);
            }
        }

        public async Task SendContentToTelegramWithTranslateText(IConfiguration conf, ITelegramBotClient bot, ChatId chatId, FlatInfoClientModel flat, CancellationToken cancel, bool isForAdmin)
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
                    await SendContentToTelegramWithTranslateText(chatId, flat, cancel, isForAdmin);
                }
                else
                {
                    await OnSendMediaGroupExceptionResponse(flat, cancel);
                }
            }
            catch
            {
                await OnSendMediaGroupExceptionResponse(flat, cancel);
            }
        }

        private async Task OnSendMediaGroupExceptionResponse(FlatInfoClientModel flat, CancellationToken cancel)
        {
            await _bot.SendTextMessageAsync(
                chatId: _conf.GetSection("BotConfiguration")["BotId"] ?? throw new InvalidOperationException(),
                text: MessageToAdminManager.GetMessageAfterExceptionWithSendMediaGroupAsyncToTelegram(flat.Id),
                parseMode: ParseMode.Html,
                cancellationToken: cancel);

            _publisher.AddDatesForTelegramException(flat.Id, DateTime.Now);
        }

        private async Task SendFlatsWhileExistAvailableFlatWithDelay(long countNotViewedFlats, CancellationToken cancel)
        {
            while (countNotViewedFlats != 0)
            {
                var flat = _informer.GetAvailableFlat();

                var channelName = _informer.GetIdChannelWithLastCheckDate();

                if (flat.LinksOfImages.Count > 2 && !_informer.IsPostedSameFlatLastHourAndIncreaseNumberOfMentionedPhoneIsPosted(flat))
                {
                    await Task.Delay(60 * 1000, cancel);
                    await SendContentToTelegramWithTranslateText(channelName, flat, cancel, false);
                    _publisher.AddDateOfTelegramPublication(flat.Id, DateTime.Now);
                }
                else
                {
                    _publisher.AddDateOfRefusePublication(flat.Id, DateTime.Now);
                }

                countNotViewedFlats = _informer.GetCountNotViewedFlats();
            }
        }

        private async Task SendFlatsWhileExistAvailableFlatWithDelay(IConfiguration conf, ITelegramBotClient bot, IFlatInfoService informer, IFlatPublicationService publisher, long countNotViewedFlats, CancellationToken cancel)
        {
            while (countNotViewedFlats != 0)
            {
                var flat = informer.GetAvailableFlat();

                var channelName = informer.GetIdChannelWithLastCheckDate();

                if (flat.LinksOfImages.Count > 2 && !informer.IsPostedSameFlatLastHourAndIncreaseNumberOfMentionedPhoneIsPosted(flat))
                {
                    await Task.Delay(60 * 1000, cancel);
                    await SendContentToTelegramWithTranslateText(conf, bot, channelName, flat, cancel, false);
                    publisher.AddDateOfTelegramPublication(flat.Id, DateTime.Now);
                }
                else
                {
                    publisher.AddDateOfRefusePublication(flat.Id, DateTime.Now);
                }

                countNotViewedFlats = informer.GetCountNotViewedFlats();
            }
        }

        private InlineKeyboardMarkup GetKeyboardWithChoose(FlatInfoClientModel flat, string channelName)
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

        private async Task<Message> SendTextMessageAsync(Message message, CancellationToken cancellationToken, string text)
        {
            return await _bot.SendTextMessageAsync(
                chatId: message.Chat.Id,
                text: text,
                parseMode: ParseMode.Html,
                cancellationToken: cancellationToken);
        }

        private async Task<Message> SendTextMessageAsync(Message message, CancellationToken cancellationToken, string text, IReplyMarkup replyMarkup)
        {
            return await _bot.SendTextMessageAsync(
                chatId: message.Chat.Id,
                text: text,
                parseMode: ParseMode.Html,
                cancellationToken: cancellationToken,
                replyMarkup: replyMarkup);
        }

        private async Task<Message> SendTextMessageAsync(ITelegramBotClient bot, Message message, CancellationToken cancellationToken, string text)
        {
            return await bot.SendTextMessageAsync(
                chatId: message.Chat.Id,
                text: text,
                parseMode: ParseMode.Html,
                cancellationToken: cancellationToken);
        }
    }
}