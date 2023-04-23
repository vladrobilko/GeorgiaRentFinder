using DataManagement.Models;
using Microsoft.EntityFrameworkCore;
using Telegram.Bot;
using TelegramBotApi.Models;
using TelegramBotApi.Services;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        services.Configure<BotConfiguration>(
            context.Configuration.GetSection(BotConfiguration.Configuration));

        services.AddHttpClient("telegram_bot_client")
                .AddTypedClient<ITelegramBotClient>((httpClient, sp) =>
                {
                    BotConfiguration? botConfig = sp.GetConfiguration<BotConfiguration>();
                    TelegramBotClientOptions options = new(botConfig.BotToken);
                    return new TelegramBotClient(options, httpClient);
                });

        services.AddScoped<UpdateHandler>();
        services.AddScoped<ReceiverService>();
        services.AddHostedService<PollingService>();
        services.AddHostedService<BotStartService>();
        services.AddDbContext<RentfinderdbContext>(options =>
        options.UseNpgsql(context.Configuration.GetSection("ConnectionStrings")["ConnectionString"]));
    })
    .Build();

await host.RunAsync();