using Application.Interfaces;
using Application.Interfaces.Repository;
using Application.Services;
using DataManagement.Models;
using DataManagement.Repositories;
using Microsoft.EntityFrameworkCore;
using Telegram.Bot;
using TelegramBotApi.Models;
using TelegramBotApi.Services;
using TelegramBotApi.Services.Managers;

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

        services.AddScoped<IFlatFindService, FlatFindingService>();
        services.AddScoped<IFlatRepository, FlatRepository>();
        services.AddScoped<IChannelInfoRepository, ChannelInfoRepository>();
        services.AddScoped<IFlatPublicationService, FlatPublicationService>();
        services.AddScoped<IFlatInfoService, FlatInfoService>();

        services.AddScoped<OnAdminMassageManager>();
        services.AddScoped<OnUserMassageManager>();
        services.AddScoped<OnAdminCallbackQueryManager>();
        services.AddScoped<OnUserCallbackQueryManager>();

        services.AddDbContext<RentFinderDbContext>(options =>
                options.UseNpgsql(context.Configuration.GetSection("ConnectionStrings")["ConnectionString"]));
    })
    .Build();


await host.RunAsync();