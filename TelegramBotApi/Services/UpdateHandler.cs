using System.ComponentModel.DataAnnotations;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InlineQueryResults;
using Telegram.Bot.Types.ReplyMarkups;
using WebScraper;
using WebScraper.Models;
using WebScraper.SsDotGe;

namespace TelegramBotApi.Services;

public class UpdateHandler : IUpdateHandler
{
    private readonly ITelegramBotClient _botClient;
    private readonly ILogger<UpdateHandler> _logger;
    private readonly IConfiguration _configuration;

    public UpdateHandler(ITelegramBotClient botClient, ILogger<UpdateHandler> logger, IConfiguration configuration)
    {
        _botClient = botClient;
        _logger = logger;
        _configuration = configuration;
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
            await botClient.SendTextMessageAsync(update.Message.Chat.Id, "Sorry, you are not admin to use this bot.",
                cancellationToken: cancellationToken);
    }

    private async Task BotOnMessageReceived(Message message, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Receive message type: {MessageType}", message.Type);
        if (message.Text is not { } messageText)
            return;

        var action = messageText.Split(' ')[0] switch
        {
            "FindSuitAdjaraFlats" => FindSuitAdjaraFlats(_botClient, message, cancellationToken),
            "/GetLastAvailableFlat" => GetLastAvailableFlat(_botClient, message, cancellationToken),
            "/throw" => FailingHandler(message, cancellationToken),
            _ => Usage(_botClient, message, cancellationToken),
        };

        Message sentMessage = await action;
        _logger.LogInformation("The message was sent with id: {SentMessageId}", sentMessage.MessageId);

        static async Task<Message> GetLastAvailableFlat(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
        {
            await botClient.SendChatActionAsync(
                message.Chat.Id,
                ChatAction.UploadPhoto,
                cancellationToken: cancellationToken);

            //var htmlScraper = new FlatScraperSsDotGe();
            // var apartmentPageOne = htmlScraper.ScrapPageWithAllFlats(AdjaraMunicipallyLinksSsDotGe.GetKobuletiLink(1));
            // make an example for test

            var images = new List<string>
            {
                "https://static.ss.ge/15_661e24c3-fc19-4847-ae52-9c03785ee18b.jpg",
                "https://static.ss.ge/0_6d6e818b-5f24-4abb-b228-3ee73f878e35.jpg",
                "https://static.ss.ge/1_9d6570a4-2ca8-49df-87da-5728eb9702f8.jpg",
                "https://static.ss.ge/13_16f7fe52-2b82-49ea-aa7d-aa06b7bc0404.jpg",
                "https://static.ss.ge/8_2c22368e-7489-4ec7-a7a7-b815f17d5445.jpg",
                "https://static.ss.ge/7_5bd8d93e-7dc0-48cc-8755-dcfe7cbbf205.jpg",
                "https://static.ss.ge/19_fe5efea4-b0c8-4296-8bfd-69c7cd4cb4a3.jpg",
                "https://static.ss.ge/20210630/1_e2b99903-867d-4b5f-8301-9fde7ae3376e.jpg",
                "https://static.ss.ge/20210630/13_251f32ee-b5a8-46a5-aea9-fbcb9deb3b22.jpg",
                "https://static.ss.ge/20210630/16_3e25d727-f39a-42b0-a158-57ad2d52f10d.jpg"
            };

            var testFlat = new FlatInfoModel("1 room Flat for rent.  Kobulet",
                300,
                new DateTime(2023, 11, 12),
                " For rent in Kobuleti, 100 meters from the sea, on Davit Aghmashenebeli Street, in Pichvnar, a 36 sq.m. isolated studio apartment. with kitchen, furniture and appliances. With 40-inch LED TV, cable channels, wi-fi, air conditioner. With 24-hour security. This price includes utility bills. ",
           new FlatPhoneTracker(){PhoneNumber = "557 73 72 21", CountMentionsOnSites = 35},
                images,
                "https://ss.ge/en/real-estate/1-room-flat-for-rent-kobuleti-3320498",
                4089,
                new FlatCoordinate(43.33, 44.77));

            var countLinks = testFlat.LinksOfImages.Count;

            var photos = new IAlbumInputMedia[countLinks];

            for (var i = 0; i < countLinks; i++)
            {
                if (i == 0)
                {
                    photos[i] = new InputMediaPhoto(testFlat.LinksOfImages[i])
                    {
                        Caption = $"{testFlat.Title}\n\n" +

                                  $"<strong>Cost:</strong> {testFlat.Cost} $\n\n" +

                                  $"<strong>Views on site:</strong> {testFlat.ViewsOnSite}\n" +
                                  $"<strong>Date of public:</strong> {testFlat.Date:dd/MM/yyyy HH:mm}\n" +
                                  $"<strong>Description:</strong> {testFlat.Description}\n\n" +

                                  $"<strong>Location:</strong><a href=\"https://www.google.com/maps/search/?api=1&query={testFlat.FlatCoordinate.Latitude},{testFlat.FlatCoordinate.Longitude}\"> link</a>\n" +
                                  $"<strong>Web page:</strong><a href=\"{testFlat.PageLink}\"> link</a>\n" +
                                  $"<strong>Mobile phone:</strong> {testFlat.FlatPhoneTracker.PhoneNumber}\n\n" +
                                  $"<strong>Maybe it's a realtor:</strong> <ins>The number was mentioned {testFlat.FlatPhoneTracker.CountMentionsOnSites} times</ins>",
                        ParseMode = ParseMode.Html
                    };
                }

                else photos[i] = new InputMediaPhoto(testFlat.LinksOfImages[i]);
            }

            await botClient.SendMediaGroupAsync(
                chatId: message.Chat.Id,
                photos,
                cancellationToken: cancellationToken);

            return await botClient.SendTextMessageAsync(message.Chat.Id, "All apartments in list", cancellationToken: cancellationToken);

        }

        static async Task<Message> FindSuitAdjaraFlats(ITelegramBotClient botClient, Message message,
            CancellationToken cancellationToken)
        {
            throw new IndexOutOfRangeException();
        }

        static async Task<Message> Usage(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
        {
            const string usage = "Usage:\n"
                                 + "/FindSuitAdjaraFlats\n" +
                                    "/GetLastAvailableFlat";

            return await botClient.SendTextMessageAsync(
                chatId: message.Chat.Id,
                text: usage,
                replyMarkup: new ReplyKeyboardRemove(),
                cancellationToken: cancellationToken);
        }

        static Task<Message> FailingHandler(Message message, CancellationToken cancellationToken)
        {
            throw new IndexOutOfRangeException();
        }
    }



    private bool IsAdmin(Update update)
    {
        return update.Message != null
               && (update.Message.Chat.Username == _configuration.GetSection("BotConfiguration")["AdminUserName"]
                   || update.Message.Chat.Id.ToString() == _configuration.GetSection("BotConfiguration")["BotId"]);
    }

    // Process Inline Keyboard callback data
    private async Task BotOnCallbackQueryReceived(CallbackQuery callbackQuery, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Received inline keyboard callback from: {CallbackQueryId}", callbackQuery.Id);

        await _botClient.AnswerCallbackQueryAsync(
            callbackQueryId: callbackQuery.Id,
            text: $"Received {callbackQuery.Data}",
            cancellationToken: cancellationToken);

        await _botClient.SendTextMessageAsync(
            chatId: callbackQuery.Message!.Chat.Id,
            text: $"Received {callbackQuery.Data}",
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