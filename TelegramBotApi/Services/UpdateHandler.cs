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
    private readonly IFlatService _flatService;
    private const string AppUsage = "AppUsage:\n"
                         + "/FindSuitAdjaraFlats\n" +
                         "/GetLastAvailableAdjaraFlat";

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
            await botClient.SendTextMessageAsync(long.Parse(_configuration.GetSection("BotConfiguration")["BotId"]), "Sorry, you are not an admin to use this bot.",
                cancellationToken: cancellationToken);
    }

    private async Task BotOnMessageReceived(Message message, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Receive message type: {MessageType}", message.Type);
        if (message.Text is not { } messageText)
            return;

        var action = messageText.Split(' ')[0] switch
        {
            "/FindSuitAdjaraFlats" => FindSuitAdjaraFlats(_botClient, _flatService, _configuration, message, cancellationToken),
            "/GetLastAvailableAdjaraFlat" => GetLastAvailableAdjaraFlat(_botClient, _flatService, _configuration, message, cancellationToken),
            "/throw" => FailingHandler(message, cancellationToken),
            _ => Usage(_botClient, message, cancellationToken),
        };

        Message sentMessage = await action;
        _logger.LogInformation("The message was sent with Id: {SentMessageId}", sentMessage.MessageId);

        static async Task<Message> GetLastAvailableAdjaraFlat(ITelegramBotClient botClient, IFlatService flatService,
            IConfiguration configuration, Message message, CancellationToken cancellationToken)
        {
            var flat = flatService.GetAvailableFlat(long.Parse(configuration.GetSection("AdjaraChannel")["ChannelId"]));
            // maybe need without channel ID here? to make this method common__for it I need here get last founded flat date

            if (flat == null)
            {
                return await botClient.SendTextMessageAsync(
                    chatId: message.Chat.Id,
                    text: $"No free flats\n\n{AppUsage}",
                    cancellationToken: cancellationToken);
            }

            await botClient.SendMediaGroupAsync(
                chatId: message.Chat.Id,
                GetAlbumInputMediaToPost(flat),
                cancellationToken: cancellationToken);

            InlineKeyboardMarkup inlineKeyboard = new(
                new[]
                {
                    new []
                    {
                        InlineKeyboardButton.WithCallbackData("✅✅Post✅✅",$"post_{flat.Id}"),
                        InlineKeyboardButton.WithCallbackData("❌DON'T post❌",$"no post_{flat.Id}"),
                    }
                });

            return await botClient.SendTextMessageAsync(
                chatId: message.Chat.Id,
                text: "Choose",
                replyMarkup: inlineKeyboard,
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
                text: $"There are <ins><strong>{countNotViewedFlats} NOT distributed flats.</strong></ins> \n" +
                $"You need to do this: /GetLastAvailableAdjaraFlat",
                parseMode: ParseMode.Html,
                replyMarkup: new ReplyKeyboardRemove(),
                    cancellationToken: cancellationToken);
            }

            flatService.FindAndSaveSuitAdjaraFlats(long.Parse(configuration.GetSection("AdjaraChannel")["ChannelId"]));

            countNotViewedFlats = flatService.GetCountNotViewedFlats();

            return await botClient.SendTextMessageAsync(
                chatId: message.Chat.Id,
                text: $"<ins><strong>{countNotViewedFlats} flats founded </strong></ins> \n" +
                      $"You need to do this: /GetLastAvailableAdjaraFlat",
                parseMode: ParseMode.Html,
                replyMarkup: new ReplyKeyboardRemove(),
                cancellationToken: cancellationToken);
        }

        static async Task<Message> Usage(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
        {
            return await botClient.SendTextMessageAsync(
                chatId: message.Chat.Id,
                text: AppUsage,
                replyMarkup: new ReplyKeyboardRemove(),
                cancellationToken: cancellationToken);
        }

        static Task<Message> FailingHandler(Message message, CancellationToken cancellationToken)
        {
            throw new IndexOutOfRangeException();
        }
    }

    private static IAlbumInputMedia[] GetAlbumInputMediaToPost(FlatInfoClientModel flat)
    {
        var photos = new IAlbumInputMedia[flat.LinksOfImages.Count];

        for (var i = 0; i < flat.LinksOfImages.Count; i++)
        {
            if (i == 0)
            {
                photos[i] = new InputMediaPhoto(flat.LinksOfImages[i])
                {
                    Caption = flat.ToTelegramCaption(),
                    ParseMode = ParseMode.Html
                };
            }

            else photos[i] = new InputMediaPhoto(flat.LinksOfImages[i]);
        }

        return photos;
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

    private async Task BotOnCallbackQueryReceived(CallbackQuery callbackQuery, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Received inline keyboard callback from: {CallbackQueryId}", callbackQuery.Id);

        var callBackInfo = callbackQuery.Data.Split("_");

        var infoData = callBackInfo[0];
        var flatId = callBackInfo[1];
        string textResponseToBot = "Default";

        var flat = _flatService.GetFlatById(long.Parse(flatId));

        if (infoData == "post")
        {
            var photos = new IAlbumInputMedia[flat.LinksOfImages.Count];

            for (var i = 0; i < flat.LinksOfImages.Count; i++)
            {
                if (i == 0)
                {
                    photos[i] = new InputMediaPhoto(flat.LinksOfImages[i])
                    {
                        Caption = flat.ToTelegramCaption(),
                        ParseMode = ParseMode.Html
                    };
                }

                else photos[i] = new InputMediaPhoto(flat.LinksOfImages[i]);
            }

            await _botClient.SendMediaGroupAsync(
                chatId: _configuration.GetSection("AdjaraChannel")["ChannelName"],
                photos,
                cancellationToken: cancellationToken);

            _flatService.AddDateOfTelegramPublication(flat.Id, DateTime.UtcNow);

            textResponseToBot = $"<ins><strong>The post has been sent!</strong></ins>\n{AppUsage}";
        }

        else if (infoData == "no post")
        {
            textResponseToBot = $"<ins><strong>The post has NOT been sent!</strong></ins>\n{AppUsage}";

            _flatService.AddDateOfRefusePublication(flat.Id, DateTime.UtcNow);
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
}