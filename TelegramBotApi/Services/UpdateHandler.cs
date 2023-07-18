using Application.Interfaces;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;

namespace TelegramBotApi.Services;

public class UpdateHandler : IUpdateHandler
{
    private readonly ITelegramBotClient _botClient;
    private readonly ILogger<UpdateHandler> _logger;
    private readonly IConfiguration _configuration;
    private readonly IFlatFindService _flatFindService;
    private readonly IFlatPublicationService _flatPublicationService;

    public UpdateHandler(ITelegramBotClient botClient, ILogger<UpdateHandler> logger, IConfiguration configuration, IFlatFindService flatFindService, IFlatPublicationService flatPublicationService)
    {
        _botClient = botClient;
        _logger = logger;
        _configuration = configuration;
        _flatFindService = flatFindService;
        _flatPublicationService = flatPublicationService;
    }

    public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        if (IsAdmin(update))
        {
            var handler = update switch
            {
                { Message: { } message } => BotOnMessageReceivedFromAdmin(message, cancellationToken),
                { CallbackQuery: { } callbackQuery } => BotOnCallbackQueryReceivedFromAdmin(callbackQuery, cancellationToken),
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

    private async Task BotOnMessageReceivedFromAdmin(Message message, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Receive message type: {MessageType}", message.Type);
        if (message.Text is not { } messageText)
            return;

        var action = messageText.Split(' ')[0] switch
        {
            "/start" => OnMassageManager.BotStart(_botClient, _flatFindService, _configuration, message, cancellationToken),
            "/AdjaraSearch" => OnMassageManager.FindSuitAdjaraFlats(_botClient, _flatFindService, _configuration, message, cancellationToken),
            "/ImeretiSearch" => OnMassageManager.FindSuitImeretiFlats(_botClient, _flatFindService, _configuration, message, cancellationToken),
            "/LookFlat" => OnMassageManager.GetLastAvailableFlat(_botClient, _flatFindService,_flatPublicationService, _configuration, message, cancellationToken),
            "/AutoFlatSendingEveryHour" => OnMassageManager.AutoFlatSendingEveryHour(_botClient, _flatFindService,_flatPublicationService, _configuration, message, cancellationToken),
            _ => OnMassageManager.OnTextResponse(_botClient, message, cancellationToken),
        };

        Message sentMessage = await action;
        _logger.LogInformation("The message was sent with Id: {SentMessageId}", sentMessage.MessageId);
    }

    private async Task BotOnCallbackQueryReceivedFromAdmin(CallbackQuery callbackQuery, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Received inline keyboard callback from: {CallbackQueryId}", callbackQuery.Id);
        // logic with switch if consist <==
        await OnCallbackQueryManager.ChoosePostingFromAdmin(callbackQuery, cancellationToken, _botClient, _configuration, _flatFindService, _flatPublicationService);
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