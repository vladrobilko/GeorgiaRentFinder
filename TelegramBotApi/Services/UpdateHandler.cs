using Application.Interfaces;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;

namespace TelegramBotApi.Services;

public class UpdateHandler : IUpdateHandler
{
    private readonly ITelegramBotClient _bot;
    private readonly ILogger<UpdateHandler> _logger;
    private readonly IConfiguration _conf;
    private readonly IFlatFindService _finder;
    private readonly IFlatPublicationService _publisher;
    private readonly IFlatInfoService _informer;

    public UpdateHandler(ITelegramBotClient bot, ILogger<UpdateHandler> logger, IConfiguration conf,
        IFlatFindService finder, IFlatPublicationService publisher, IFlatInfoService informer)
    {
        _bot = bot;
        _logger = logger;
        _conf = conf;
        _finder = finder;
        _publisher = publisher;
        _informer = informer;
    }

    public async Task HandleUpdateAsync(ITelegramBotClient bot, Update update, CancellationToken cancel)
    {
        if (IsAdmin(update))
        {
            var handler = update switch
            {
                { Message: { } message } => BotOnMessageReceivedFromAdmin(message, cancel),
                { CallbackQuery: { } callbackQuery } => BotOnCallbackQueryReceivedFromAdmin(callbackQuery, cancel),
                _ => UnknownUpdateHandlerAsync(update)
            };

            await handler;
        }
        else
        {
            if (update.Message == null) throw new FormatException();

            await bot.SendTextMessageAsync(chatId: update.Message.Chat.Id,
                BotMessageManager.GetMessageForNoAdmin,
                cancellationToken: cancel);
        }
    }

    private async Task BotOnMessageReceivedFromAdmin(Message mes, CancellationToken cancel)
    {
        _logger.LogInformation("Receive mes type: {MessageType}", mes.Type);
        if (mes.Text is not { } messageText)
            return;

        var action = messageText.Split(' ')[0] switch
        {
            "/start" => OnMassageManager.BotStart(_bot, _informer, mes, cancel),
            "/AdjaraSearch" => OnMassageManager.FindSuitAdjaraFlats(_bot, _finder, _informer, _conf, mes, cancel),
            "/ImeretiSearch" => OnMassageManager.FindSuitImeretiFlats(_bot, _finder, _informer, _conf, mes, cancel),
            "/LookFlat" => OnMassageManager.GetLastAvailableFlat(_bot, _informer, _publisher, _conf, mes, cancel),
            "/AutoFlatSendingEveryHour" => OnMassageManager.AutoFlatSendingEveryHour(_bot, _finder, _informer, _publisher, _conf, mes, cancel),
            _ => OnMassageManager.OnTextResponse(_bot, mes, cancel),
        };

        Message sentMessage = await action;
        _logger.LogInformation("The mes was sent with Id: {SentMessageId}", sentMessage.MessageId);
    }

    private async Task BotOnCallbackQueryReceivedFromAdmin(CallbackQuery callback, CancellationToken cancel)
    {
        _logger.LogInformation("Received inline keyboard callback from: {CallbackQueryId}", callback.Id);
        // logic with switch if consist <==
        await OnCallbackQueryManager.ChoosePostingFromAdmin(callback, cancel, _bot, _conf, _publisher, _informer);
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