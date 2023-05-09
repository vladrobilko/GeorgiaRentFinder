using Telegram.Bot;
using Telegram.Bot.Types.ReplyMarkups;

namespace TelegramBotApi.Services
{
    public class BotStartService : IHostedService
    {
        private readonly ITelegramBotClient _botClient;
        private readonly IConfiguration _configuration;

        public BotStartService(ITelegramBotClient telegramBotClient, IConfiguration configuration)
        {
            _botClient = telegramBotClient;
            _configuration = configuration;
        }
        public Task StartAsync(CancellationToken cancellationToken)
        {
            return _botClient.SendTextMessageAsync(
                chatId: _configuration.GetSection("BotConfiguration")["BotId"],
                text: BotMessageManager.GetUsageWithTimeNow(),
                replyMarkup: new ReplyKeyboardRemove(),
                cancellationToken: cancellationToken);
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
