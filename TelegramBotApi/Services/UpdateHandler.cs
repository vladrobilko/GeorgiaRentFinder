using Application.Interfaces;
using Microsoft.Extensions.Hosting;
using System.Net;
using System.Threading;
using Application.Converters;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InlineQueryResults;
using Telegram.Bot.Types.ReplyMarkups;
using WebScraper.Converters;
using WebScraper.Models;

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

            if (flat == null)
            {
                return await botClient.SendTextMessageAsync(
                    chatId: message.Chat.Id,
                    text: "No free flats",
                    cancellationToken: cancellationToken);
            }
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

            await botClient.SendMediaGroupAsync(
                chatId: message.Chat.Id,
                photos,
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
            var carNotDistributedFlats = flatService.GetCountNotViewedFlats();

            if (carNotDistributedFlats != 0)
            {
                return await botClient.SendTextMessageAsync(
                chatId: message.Chat.Id,
                text: $"There are <ins><strong>{carNotDistributedFlats} NOT distributed flats.</strong></ins> \n" + 
                $"You need to do this: /GetLastAvailableAdjaraFlat",
                parseMode:ParseMode.Html,
                replyMarkup: new ReplyKeyboardRemove(),
                    cancellationToken: cancellationToken);
            }

            flatService.FindAndSaveSuitAdjaraFlats(long.Parse(configuration.GetSection("AdjaraChannel")["ChannelId"]));

            carNotDistributedFlats = flatService.GetCountNotViewedFlats();

            return await botClient.SendTextMessageAsync(
                chatId: message.Chat.Id,
                text: $"<ins><strong>{carNotDistributedFlats} flats founded </strong></ins> \n" +
                      $"You need to do this: /GetLastAvailableAdjaraFlat",
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

    private bool IsAdmin(Update update)
    {
        if (update.CallbackQuery != null)
        {
            return update.CallbackQuery.From.Username == _configuration.GetSection("BotConfiguration")["AdminUserName"];
        }
        return update.Message != null
               && (update.Message.Chat.Username == _configuration.GetSection("BotConfiguration")["AdminUserName"]
                   || update.Message.Chat.Id.ToString() == _configuration.GetSection("BotConfiguration")["BotId"]);
    }

    // Process Inline Keyboard callback data
    private async Task BotOnCallbackQueryReceived(CallbackQuery callbackQuery, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Received inline keyboard callback from: {CallbackQueryId}", callbackQuery.Id);

        var callBackInfo = callbackQuery.Data.Split("_");

        var infoData = callBackInfo[0];
        var flatId = callBackInfo[1];

        string response = "Default";

        if (infoData == "post")
        {         
            //test flat
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
            var photos = new IAlbumInputMedia[images.Count];
            for (var i = 0; i < images.Count; i++)
            {
                if (i == 0)
                {
                    photos[i] = new InputMediaPhoto(images[i])
                    {
                        /*Caption = new FlatInfoModel().ToTelegramCaption(),
                        ParseMode = ParseMode.Html*/
                    };
                }

                else photos[i] = new InputMediaPhoto(images[i]);
            }
            //test flat

            await _botClient.SendMediaGroupAsync(
                chatId: _configuration.GetSection("AdjaraChannel")["ChannelName"], // I need here give number above
                photos,
                cancellationToken: cancellationToken);

            response = $"<ins><strong>The post has been sent!</strong></ins>\n{AppUsage}";
            //add to db
        }

        else if (infoData == "no post")
        {
            response = $"<ins><strong>The post has NOT been sent!</strong></ins>\n{AppUsage}";
            //logic to db
        }

        await _botClient.SendTextMessageAsync(
            chatId: callbackQuery.Message.Chat.Id,
            text: response,
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