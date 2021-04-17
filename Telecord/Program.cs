using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using TehGM.Telecord.Discord;
using TehGM.Telecord.Telegram;
using TehGM.Telecord.Utilities;

namespace TehGM.Telecord
{
    class Program
    {
        private static IServiceProvider _services;

        private static async Task Main(string[] args)
        {
            // handle exiting
            AppDomain.CurrentDomain.ProcessExit += (sender, e) => OnExit();

            // build configuration
            IConfiguration config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: false)
                .AddJsonFile("appsecrets.json", optional: true)
                .AddEnvironmentVariables()
                .AddCommandLine(args)
                .Build();

            try
            {
                // prepare DI container
                IServiceCollection serviceCollection = ConfigureServices(config);
                _services = serviceCollection.BuildServiceProvider();

                // initialize Telegram client
                ITelegramClient telegramClient = _services.GetRequiredService<ITelegramClient>();
                telegramClient.Start();

                // initialize Discord client
                IDiscordClient discordClient = _services.GetRequiredService<IDiscordClient>();
                await discordClient.StartClientAsync();

                // wait forever to prevent window closing
                await Task.Delay(-1).ConfigureAwait(false);
            }
            finally
            {
                OnExit();
            }
        }

        private static IServiceCollection ConfigureServices(IConfiguration configuration)
        {
            IServiceCollection services = new ServiceCollection();

            // Logging
            Logging.ConfigureLogging(configuration);
            services.AddLogging()
                .AddSingleton<ILoggerFactory>(new LoggerFactory()
                        .AddSerilog(Log.Logger, dispose: true));

            // Discord
            services.AddSingleton<IDiscordClient, Discord.Services.TelecordDiscordClient>()
                .Configure<DiscordOptions>(configuration.GetSection("Discord"));

            // Telegram
            services.AddSingleton<ITelegramClient, Telegram.Services.TelecordTelegramClient>()
                .Configure<TelegramOptions>(configuration.GetSection("Telegram"));

            return services;
        }

        private static void OnExit()
        {
            try { Log.CloseAndFlush(); } catch { }
            try
            {
                if (_services is IDisposable disposableServices)
                    disposableServices.Dispose();
            }
            catch { }
        }
    }
}
