using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TehGM.Telecord.Discord;
using TehGM.Telecord.Telegram;

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

            // Discord
            services.Configure<DiscordOptions>(configuration.GetSection("Discord"));

            // Telegram
            services.Configure<TelegramOptions>(configuration.GetSection("Telegram"));

            return services;
        }

        private static void OnExit()
        {
            try
            {
                if (_services is IDisposable disposableServices)
                    disposableServices.Dispose();
            }
            catch { }
        }
    }
}
