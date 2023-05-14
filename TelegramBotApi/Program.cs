using Application.Interfaces;
using Application.Interfaces.Repository;
using Application.Services;
using DataManagement.Models;
using DataManagement.Repositories;
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
                    BotConfiguration botConfig = sp.GetConfiguration<BotConfiguration>();
                    TelegramBotClientOptions options = new(botConfig.BotToken);
                    return new TelegramBotClient(options, httpClient);
                });

        services.AddScoped<UpdateHandler>();
        services.AddScoped<ReceiverService>();
        services.AddHostedService<PollingService>();

        services.AddScoped<IFlatService, FlatService>();
        services.AddScoped<IFlatRepository, FlatRepository>();
        services.AddScoped<IChannelInfoRepository, ChannelInfoRepository>();

        services.AddDbContext<RentFinderDbContext>(options =>
        options.UseNpgsql(context.Configuration.GetSection("ConnectionStrings")["ConnectionString"]));
    })
    .Build();


await host.RunAsync();