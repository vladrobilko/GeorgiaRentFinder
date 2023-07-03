using System.Timers;
using Application.Interfaces;
using Application.Converters;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InlineQueryResults;
using Telegram.Bot.Types.ReplyMarkups;
using Application.Models;

namespace TelegramBotApi.Services;

public class UpdateHandler : IUpdateHandler
{
    private readonly ITelegramBotClient _botClient;
    private readonly ILogger<UpdateHandler> _logger;
    private readonly IConfiguration _configuration;
    private static IFlatService _flatService;

    public UpdateHandler(ITelegramBotClient botClient, ILogger<UpdateHandler> logger, IConfiguration configuration, IFlatService flatService)
    {
        _botClient = botClient;
        _logger = logger;
        _configuration = configuration;
        _flatService = flatService;
    }

    public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        if (IsAdmin(update))
        {
            var handler = update switch
            {
                { Message: { } message } => BotOnMessageReceived(message, cancellationToken),
                { EditedMessage: { } message } => BotOnMessageReceived(message, cancellationToken),
                { CallbackQuery: { } callbackQuery } => BotOnCallbackQueryReceived(callbackQuery, cancellationToken),
                { InlineQuery: { } inlineQuery } => BotOnInlineQueryReceived(inlineQuery, cancellationToken),
                { ChosenInlineResult: { } chosenInlineResult } => BotOnChosenInlineResultReceived(chosenInlineResult, cancellationToken),
                _ => UnknownUpdateHandlerAsync(update, cancellationToken)
            };

            await handler;
        }
        else
            await botClient.SendTextMessageAsync(chatId: update.Message.Chat.Id,
                    "Sorry, you are not an admin to use this bot.",
                    cancellationToken: cancellationToken);
    }

    private async Task BotOnMessageReceived(Message message, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Receive message type: {MessageType}", message.Type);
        if (message.Text is not { } messageText)
            return;

        var action = messageText.Split(' ')[0] switch
        {
            "/start" => BotStart(_botClient, _flatService, _configuration, message, cancellationToken),
            "/AdjaraSearch" => FindSuitAdjaraFlats(_botClient, _flatService, _configuration, message, cancellationToken),
            "/ImeretiSearch" => FindSuitImeretiFlats(_botClient, _flatService, _configuration, message, cancellationToken),
            "/LookFlat" => GetLastAvailableFlat(_botClient, _flatService, _configuration, message, cancellationToken),
            "/AutoFlatSendingEveryHour" => AutoFlatSendingEveryHour(_botClient, _flatService, _configuration, message, cancellationToken),
            "/throw" => FailingHandler(message, cancellationToken),
            _ => OnTextResponse(_botClient, message, cancellationToken),
        };

        Message sentMessage = await action;
        _logger.LogInformation("The message was sent with Id: {SentMessageId}", sentMessage.MessageId);

        static async Task<Message> BotStart(ITelegramBotClient botClient, IFlatService flatService,
            IConfiguration configuration, Message message, CancellationToken cancellationToken)
        {
            return await botClient.SendTextMessageAsync(
                chatId: message.Chat.Id,
                text: BotMessageManager.GetMessageFlatCountInfo(flatService.GetCountNotViewedFlats()),
                parseMode: ParseMode.Html,
                cancellationToken: cancellationToken);
        }

        static async Task<Message> GetLastAvailableFlat(ITelegramBotClient botClient, IFlatService flatService,
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

            await SendContentToTelegramWithTranslateText(botClient, message.Chat.Id, flat, configuration, cancellationToken, true);

            return await botClient.SendTextMessageAsync(
                chatId: message.Chat.Id,
                text: "Choose",
                replyMarkup: GetKeyboardWithChoose(flat, channelName),
                cancellationToken: cancellationToken);
        }

        static async Task<Message> FindSuitAdjaraFlats(ITelegramBotClient botClient, IFlatService flatService,
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

            flatService.FindAndSaveSuitAdjaraFlats(long.Parse(configuration.GetSection("AdjaraChannel")["ChannelId"]));

            return await botClient.SendTextMessageAsync(
                chatId: message.Chat.Id,
                text: BotMessageManager.GetMessageFlatCountInfo(flatService.GetCountNotViewedFlats()),
                parseMode: ParseMode.Html,
                replyMarkup: new ReplyKeyboardRemove(),
                cancellationToken: cancellationToken);
        }

        static async Task<Message> FindSuitImeretiFlats(ITelegramBotClient botClient, IFlatService flatService,
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

            flatService.FindAndSaveSuitImeretiFlats(long.Parse(configuration.GetSection("ImeretiChannel")["ChannelId"]));

            return await botClient.SendTextMessageAsync(
                chatId: message.Chat.Id,
                text: BotMessageManager.GetMessageFlatCountInfo(flatService.GetCountNotViewedFlats()),
                parseMode: ParseMode.Html,
                replyMarkup: new ReplyKeyboardRemove(),
                cancellationToken: cancellationToken);
        }

        static async Task<Message> AutoFlatSendingEveryHour(ITelegramBotClient botClient, IFlatService flatService,
            IConfiguration configuration, Message message, CancellationToken cancellationToken)
        {
            if (flatService.GetCountNotViewedFlats() != 0) throw new NotSupportedException();

            await botClient.SendTextMessageAsync(
                chatId: message.Chat.Id,
                text: BotMessageManager.GetMessageForStartAutoFlatSendingEveryHour(),
                parseMode: ParseMode.Html,
                cancellationToken: cancellationToken);

            while (!cancellationToken.IsCancellationRequested)
            {
                long countProcessedFlats = 0;

                flatService.FindAndSaveSuitAdjaraFlats(long.Parse(configuration.GetSection("AdjaraChannel")["ChannelId"]));
                var countNotViewedFlats = flatService.GetCountNotViewedFlats();
                countProcessedFlats += countNotViewedFlats;
                await SendFlatsWhileExistAvailableFlat(countNotViewedFlats, flatService, botClient, configuration, cancellationToken);

                flatService.FindAndSaveSuitImeretiFlats(long.Parse(configuration.GetSection("ImeretiChannel")["ChannelId"]));
                countNotViewedFlats = flatService.GetCountNotViewedFlats();
                countProcessedFlats += countNotViewedFlats;
                await SendFlatsWhileExistAvailableFlat(countNotViewedFlats, flatService, botClient, configuration, cancellationToken);

                await botClient.SendTextMessageAsync(
                    chatId: message.Chat.Id,
                    text: BotMessageManager.GetMessageCountOfProcessedFlats(countProcessedFlats),
                    parseMode: ParseMode.Html,
                    cancellationToken: cancellationToken);

                await Task.Delay(60 * 60 * 1000, cancellationToken);
            }

            return message;
        }

        static async Task<Message> OnTextResponse(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
        {
            return await botClient.SendTextMessageAsync(
                chatId: message.Chat.Id,
                text: BotMessageManager.GetMessageAfterOnlyTextSending(),
                replyMarkup: new ReplyKeyboardRemove(),
                cancellationToken: cancellationToken);
        }

        static Task<Message> FailingHandler(Message message, CancellationToken cancellationToken)
        {
            throw new IndexOutOfRangeException();
        }
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
                await SendContentToTelegramWithTranslateText(botClient, channelName, flat, configuration,
                    cancellationToken, false);
                _flatService.AddDateOfTelegramPublication(flat.Id, DateTime.Now);
            }
            else
            {
                _flatService.AddDateOfRefusePublication(flat.Id, DateTime.Now);
            }


            countNotViewedFlats = flatService.GetCountNotViewedFlats();
        }
    }

    private static async Task SendContentToTelegramWithTranslateText(ITelegramBotClient botClient, ChatId chatId,
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
        catch (Exception e)
        {
            _flatService.AddDatesForTelegramException(flat.Id, DateTime.Now);
        }
    }

    private async Task BotOnCallbackQueryReceived(CallbackQuery callbackQuery, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Received inline keyboard callback from: {CallbackQueryId}", callbackQuery.Id);

        var callBackInfo = callbackQuery.Data.Split("_");

        var infoData = callBackInfo[0];
        var flatId = callBackInfo[1];
        var channelName = callBackInfo[2];

        string textResponseToBot = "Default";

        var flat = _flatService.GetFlatById(long.Parse(flatId));

        if (infoData == "post")
        {
            await SendContentToTelegramWithTranslateText(_botClient, channelName, flat, _configuration, cancellationToken, false);

            _flatService.AddDateOfTelegramPublication(flat.Id, DateTime.Now);

            textResponseToBot = BotMessageManager.GetMessageAfterPost(_flatService.GetCountNotViewedFlats());
        }

        else if (infoData == "no post")
        {
            _flatService.AddDateOfRefusePublication(flat.Id, DateTime.Now);

            textResponseToBot = BotMessageManager.GetMessageAfterRefusePost(_flatService.GetCountNotViewedFlats());
        }

        await _botClient.SendTextMessageAsync(
            chatId: callbackQuery.Message.Chat.Id,
            text: textResponseToBot,
            parseMode: ParseMode.Html,
            replyMarkup: new InlineKeyboardMarkup(new InlineKeyboardButton[][] { }),
            cancellationToken: cancellationToken);
    }

    #region Inline Mode

    private async Task BotOnInlineQueryReceived(InlineQuery inlineQuery, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Received inline query from: {InlineQueryFromId}", inlineQuery.From.Id);

        InlineQueryResult[] results = {
            new InlineQueryResultArticle(
                id: "1",
                title: "TgBots",
                inputMessageContent: new InputTextMessageContent("hello"))
        };

        await _botClient.AnswerInlineQueryAsync(
            inlineQueryId: inlineQuery.Id,
            results: results,
            cacheTime: 0,
            isPersonal: true,
            cancellationToken: cancellationToken);
    }

    private async Task BotOnChosenInlineResultReceived(ChosenInlineResult chosenInlineResult, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Received inline result: {ChosenInlineResultId}", chosenInlineResult.ResultId);

        await _botClient.SendTextMessageAsync(
            chatId: chosenInlineResult.From.Id,
            text: $"You chose result with Id: {chosenInlineResult.ResultId}",
            cancellationToken: cancellationToken);
    }

    #endregion

    private Task UnknownUpdateHandlerAsync(Update update, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Unknown update type: {UpdateType}", update.Type);
        return Task.CompletedTask;
    }

    public async Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
    {
        var errorMessage = exception switch
        {
            ApiRequestException apiRequestException => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
            _ => exception.ToString()
        };

        _logger.LogInformation("HandleError: {ErrorMessage}", errorMessage);

        // Cooldown in case of network connection error
        if (exception is RequestException)
            await Task.Delay(TimeSpan.FromSeconds(20), cancellationToken);
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

    private bool IsAdmin(Update update)
    {
        if (update.CallbackQuery != null)
        {
            return update.CallbackQuery.From.Username == _configuration.GetSection("BotConfiguration")["AdminUserName"];
        }
        return update.Message != null
               && update.Message.Chat.Username == _configuration.GetSection("BotConfiguration")["AdminUserName"]
               && update.Message.Chat.Id.ToString() == _configuration.GetSection("BotConfiguration")["BotId"];
    }
}