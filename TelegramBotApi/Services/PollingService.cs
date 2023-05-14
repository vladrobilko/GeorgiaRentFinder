using Telegram.Bot;
using TelegramBotApi.Abstract;

namespace TelegramBotApi.Services;

public class PollingService : PollingServiceBase<ReceiverService>
{
    public PollingService(IServiceProvider serviceProvider, ILogger<PollingService> logger, ITelegramBotClient botClient, IConfiguration configuration)
        : base(serviceProvider, logger, botClient, configuration)
    {
    }
}
