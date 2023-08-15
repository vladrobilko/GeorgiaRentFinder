using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using TelegramBotApi.Services.Managers;

namespace TelegramBotApi.Services;

public class UpdateHandler : IUpdateHandler
{
    private readonly OnAdminMassageManager _onAdminMassageManager;
    private readonly OnUserMassageManager _onUserMassageManager;
    private readonly OnAdminCallbackQueryManager _onAdminCallbackQueryManager;
    private readonly OnUserCallbackQueryManager _onUserCallbackQueryManager;

    private readonly ILogger<UpdateHandler> _logger;
    private readonly IConfiguration _conf;

    public UpdateHandler(ILogger<UpdateHandler> logger, IConfiguration conf,
        OnAdminMassageManager onAdminMassageManager, OnUserMassageManager onUserMassageManager,
        OnAdminCallbackQueryManager onAdminCallbackQueryManager, OnUserCallbackQueryManager onUserCallbackQueryManager)
    {
        _logger = logger;
        _conf = conf;
        _onAdminMassageManager = onAdminMassageManager;
        _onUserMassageManager = onUserMassageManager;
        _onAdminCallbackQueryManager = onAdminCallbackQueryManager;
        _onUserCallbackQueryManager = onUserCallbackQueryManager;
    }

    public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        if (IsAdmin(update))
        {
            var handler = update switch
            {
                { Message: { } message } => BotOnAdminMessageReceived(message, cancellationToken),
                { CallbackQuery: { } callbackQuery } => BotOnAdminCallbackQueryReceived(callbackQuery, cancellationToken),
                _ => UnknownUpdateHandlerAsync(update)
            };

            await handler;
        }
        else
        {
            var handler = update switch
            {
                { Message: { } message } => BotOnUserMessageReceived(message, cancellationToken),
                { CallbackQuery: { } callbackQuery } => BotOnUserCallbackQueryReceived(callbackQuery, cancellationToken),
                _ => UnknownUpdateHandlerAsync(update)
            };

            await handler;
        }
    }

    private async Task BotOnAdminMessageReceived(Message mes, CancellationToken cancel)
    {
        _logger.LogInformation("Receive mes type: {MessageType}", mes.Type);
        if (mes.Text is not { } messageText)
            return;

        var action = messageText.Split(' ')[0] switch
        {
            "/start" => _onAdminMassageManager.BotStart(mes, cancel),
            "/AdjaraSearch" => _onAdminMassageManager.FindSuitAdjaraFlats(mes, cancel),
            "/ImeretiSearch" => _onAdminMassageManager.FindSuitImeretiFlats(mes, cancel),
            "/TbilisiRustaviSearch" => _onAdminMassageManager.FindSuitTbilisiRustaviFlats(mes, cancel),
            "/LookFlat" => _onAdminMassageManager.GetLastAvailableFlat(mes, cancel),
            "/AutoFlatSendingEveryHour" => _onAdminMassageManager.AutoFlatSendingEveryHour(mes, cancel),
            _ => _onAdminMassageManager.OnTextResponse(mes, cancel),
        };

        Message sentMessage = await action;
        _logger.LogInformation("The mes was sent with Id: {SentMessageId}", sentMessage.MessageId);
    }
    private async Task BotOnUserMessageReceived(Message mes, CancellationToken cancel)
    {
        _logger.LogInformation("Receive mes type: {MessageType}", mes.Type);
        if (mes.Text is not { } messageText)
            return;

        var action = messageText.Split(' ')[0] switch
        {
            "/start" => _onUserMassageManager.BotStart(mes, cancel),
            "/rent" => _onUserMassageManager.Rent(mes, cancel),
            "/rentOut" => _onUserMassageManager.RentOut(mes, cancel),
            "/admin" => _onUserMassageManager.Admin(mes, cancel),
            _ => _onUserMassageManager.OnTextResponse(mes, cancel)
        };

        Message sentMessage = await action;
        _logger.LogInformation("The mes was sent with Id: {SentMessageId}", sentMessage.MessageId);
    }

    private async Task BotOnAdminCallbackQueryReceived(CallbackQuery callback, CancellationToken cancel)
    {
        _logger.LogInformation("Received inline keyboard callback from: {CallbackQueryId}", callback.Id);

        if (callback.Data != null && callback.Data.Contains("post"))
            await _onAdminCallbackQueryManager.ChooseFLatPostFromAdmin(callback, cancel);
    }
    private async Task BotOnUserCallbackQueryReceived(CallbackQuery callback, CancellationToken cancel)
    {
        _logger.LogInformation("Received inline keyboard callback from: {CallbackQueryId}", callback.Id);

        if (callback.Data != null && callback.Data.Contains("language"))
            await _onUserCallbackQueryManager.ChooseLanguageAndGiveChoiceForCity(callback, cancel);
        else if (callback.Data != null && callback.Data.Contains("cityChoice"))
            await _onUserCallbackQueryManager.ChooseCityAndGiveChoiceForAction(callback, cancel);
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
            return update.CallbackQuery.From.Username == _conf.GetSection("BotConfiguration")["AdminUserName"];
        }
        return update.Message != null
               && update.Message.Chat.Username == _conf.GetSection("BotConfiguration")["AdminUserName"]
               && update.Message.Chat.Id.ToString() == _conf.GetSection("BotConfiguration")["BotId"];
    }
}