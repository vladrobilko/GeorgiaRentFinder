using Telegram.Bot;

namespace TelegramBotApi.Abstract;

public abstract class PollingServiceBase<TReceiverService> : BackgroundService
    where TReceiverService : IReceiverService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger _logger;
    private readonly ITelegramBotClient _botClient;
    private readonly IConfiguration _configuration;

    protected PollingServiceBase(
        IServiceProvider serviceProvider,
        ILogger<PollingServiceBase<TReceiverService>> logger,
        ITelegramBotClient botClient,
        IConfiguration configuration)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _botClient = botClient;
        _configuration = configuration;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Starting polling service");

        await Start(stoppingToken);

        await DoWork(stoppingToken);
    }

    private async Task Start(CancellationToken cancellationToken)
    {
        await _botClient.SendTextMessageAsync(
            chatId: _configuration.GetSection("BotConfiguration")["BotId"] ?? throw new InvalidOperationException(),
            text: BotMessageManager.GetStartMessage(),
            cancellationToken: cancellationToken);
    }

    private async Task DoWork(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var receiver = scope.ServiceProvider.GetRequiredService<TReceiverService>();

                await receiver.ReceiveAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError("Polling failed with exception: {Exception}", ex);

                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }
        }
    }
}
