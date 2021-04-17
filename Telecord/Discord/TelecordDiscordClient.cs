using System.Threading.Tasks;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Discord;
using System;
using System.Collections.Generic;

namespace TehGM.Telecord.Discord.Services
{
    class TelecordDiscordClient : IDiscordClient
    {
        public DiscordSocketClient Client { get; }

        private readonly ILogger _log;
        private readonly IOptionsMonitor<DiscordOptions> _discordOptions;
        public event EventHandler<SocketMessage> MessageReceived;

        public TelecordDiscordClient(IOptionsMonitor<DiscordOptions> discordOptions, ILogger<TelecordDiscordClient> log, ILoggerFactory logFactory)
        {
            this._discordOptions = discordOptions;
            this._log = log;

            DiscordSocketConfig clientConfig = new DiscordSocketConfig();
            clientConfig.LogLevel = LogSeverity.Verbose;
            this.Client = new DiscordSocketClient(clientConfig);
            this.Client.Log += OnClientLog;
            this.Client.MessageReceived += OnClientMessage;

            this._discordOptions.OnChange(async _ =>
            {
                if (Client.ConnectionState == ConnectionState.Connected || Client.ConnectionState == ConnectionState.Connecting)
                {
                    await StopClientAsync().ConfigureAwait(false);
                    await StartClientAsync().ConfigureAwait(false);
                }
            });
        }

        private Task OnClientMessage(SocketMessage msg)
        {
            if (this._log.IsEnabled(LogLevel.Trace))
            {
                using IDisposable logScope = this._log.BeginScope(new Dictionary<string, object>()
                {
                    { "ID", msg.Id },
                    { "Text", msg.Content },
                    { "SenderID", msg.Author.Id },
                    { "ChannelID", msg.Channel.Id },
                    { "GuildID", (msg.Channel as SocketGuildChannel)?.Guild?.Id }
                });
                this._log.LogTrace("{Type} message {ID} received");
            }
            this.MessageReceived?.Invoke(this, msg);
            return Task.CompletedTask;
        }

        public async Task StartClientAsync()
        {
            this._log.LogInformation("Starting Discord client");
            await this.Client.LoginAsync(TokenType.Bot, _discordOptions.CurrentValue.BotToken).ConfigureAwait(false);
            await this.Client.StartAsync().ConfigureAwait(false);
        }

        public async Task StopClientAsync()
        {
            this._log.LogInformation("Stopping Discord client");
            if (this.Client.LoginState == LoginState.LoggedIn || this.Client.LoginState == LoginState.LoggingIn)
                await this.Client.LogoutAsync().ConfigureAwait(false);
            if (this.Client.ConnectionState == ConnectionState.Connected || this.Client.ConnectionState == ConnectionState.Connecting)
                await this.Client.StopAsync().ConfigureAwait(false);
        }

        private Task OnClientLog(LogMessage message)
        {
            this._log.Log(message);
            return Task.CompletedTask;
        }

        public static implicit operator DiscordSocketClient(TelecordDiscordClient client)
            => client.Client;

        public void Dispose()
        {
            try { this.Client.MessageReceived -= OnClientMessage; } catch { }
            try { this.Client.Log -= OnClientLog; } catch { }
            try { this.Client?.Dispose(); } catch { }
        }
    }
}
