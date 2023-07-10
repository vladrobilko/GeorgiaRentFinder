using Application.Interfaces;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace TelegramBotApi.Services;

public class UpdateHandler : IUpdateHandler
{
    private readonly ITelegramBotClient _botClient;
    private readonly ILogger<UpdateHandler> _logger;
    private readonly IConfiguration _configuration;
    private readonly IFlatService _flatService;

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
                { CallbackQuery: { } callbackQuery } => BotOnCallbackQueryReceived(callbackQuery, cancellationToken),
                _ => UnknownUpdateHandlerAsync(update)
            };

            await handler;
        }
        else
        {
            if (update.Message == null) throw new FormatException();
            
            await botClient.SendTextMessageAsync(chatId: update.Message.Chat.Id,
                BotMessageManager.GetMessageForNoAdmin,
                cancellationToken: cancellationToken);
        }
    }

    private async Task BotOnMessageReceived(Message message, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Receive message type: {MessageType}", message.Type);
        if (message.Text is not { } messageText)
            return;

        var action = messageText.Split(' ')[0] switch
        {
            "/start" => UpdateHandlerManager.BotStart(_botClient, _flatService, _configuration, message, cancellationToken),
            "/AdjaraSearch" => UpdateHandlerManager.FindSuitAdjaraFlats(_botClient, _flatService, _configuration, message, cancellationToken),
            "/ImeretiSearch" => UpdateHandlerManager.FindSuitImeretiFlats(_botClient, _flatService, _configuration, message, cancellationToken),
            "/LookFlat" => UpdateHandlerManager.GetLastAvailableFlat(_botClient, _flatService, _configuration, message, cancellationToken),
            "/AutoFlatSendingEveryTwoHours" => UpdateHandlerManager.AutoFlatSendingEveryTwoHours(_botClient, _flatService, _configuration, message, cancellationToken),
            _ => UpdateHandlerManager.OnTextResponse(_botClient, message, cancellationToken),
        };

        Message sentMessage = await action;
        _logger.LogInformation("The message was sent with Id: {SentMessageId}", sentMessage.MessageId);
    }

    private async Task BotOnCallbackQueryReceived(CallbackQuery callbackQuery, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Received inline keyboard callback from: {CallbackQueryId}", callbackQuery.Id);

        if (callbackQuery.Data == null || callbackQuery.Message == null) throw new NotImplementedException();

        var callBackInfo = callbackQuery.Data.Split("_");

        var infoData = callBackInfo[0];
        var flatId = callBackInfo[1];
        var channelName = callBackInfo[2];

        string textResponseToBot = "Default";

        var flat = _flatService.GetFlatById(long.Parse(flatId));

        if (infoData == "post")
        {
            await UpdateHandlerManager.SendContentToTelegramWithTranslateText(_botClient,_flatService, channelName, flat, _configuration, cancellationToken, false);

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

    private Task UnknownUpdateHandlerAsync(Update update)
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