using System;
using System.Collections.Generic;
using System.Threading;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types.Enums;

namespace TehGM.Telecord.Telegram.Services
{
    /// <summary>A wrapper for <see cref="TelegramBotClient"/> that supports options hot-reload.</summary>
    public class TelecordTelegramClient : ITelegramClient, IDisposable
    {
        public ITelegramBotClient Client { get; private set; }
        public bool IsRunning { get; private set; }
        public event EventHandler<MessageEventArgs> MessageReceived;

        private readonly ILogger _log;
        private readonly IOptionsMonitor<TelegramOptions> _options;
        private readonly IDisposable _optionsChangeRegistration;
        private readonly object _clientLock = new object();
        private readonly UpdateType[] _listenTypes = { UpdateType.Message, UpdateType.Unknown };
        private readonly CancellationTokenSource _cts = new CancellationTokenSource();

        public TelecordTelegramClient(IOptionsMonitor<TelegramOptions> options, ILogger<TelecordTelegramClient> log)
        {
            this._log = log;
            this._options = options;
            this._optionsChangeRegistration = this._options.OnChange(_ => this.Init());
            this.Init();
        }

        private void Init()
        {
            lock (_clientLock)
            {
                bool running = this.IsRunning;
                if (running)
                    this.StopInternal();
                this.DisposeInternalClient();
                this._log.LogDebug("Initializing Telegram client");
                this.Client = new TelegramBotClient(this._options.CurrentValue.BotToken);
                this.Client.OnMessage += OnMessage;
                this.Client.OnReceiveError += OnError;
                this.Client.OnReceiveGeneralError += OnGeneralError;
                if (running)
                    this.StartInternal();
            }
        }

        public void Start()
        {
            lock (_clientLock)
                this.StartInternal();
        }

        private void StartInternal()
        {
            this._log.LogInformation("Starting Telegram client");
            this.Client.StartReceiving(this._listenTypes, this._cts.Token);
            this.IsRunning = true;
        }

        private void OnMessage(object sender, MessageEventArgs e)
        {
            if (this._log.IsEnabled(LogLevel.Trace))
            {
                using IDisposable logScope = this._log.BeginScope(new Dictionary<string, object>()
                {
                    { "Type", e.Message.Type },
                    { "ID", e.Message.MessageId },
                    { "Text", e.Message.Text },
                    { "SenderID", e.Message.From.Id },
                    { "ChannelID", e.Message.Chat.Id }
                });
                this._log.LogTrace("{Type} message {ID} received");
            }
            this.MessageReceived?.Invoke(this, e);
        }

        private void OnGeneralError(object sender, ReceiveGeneralErrorEventArgs e)
            => this._log.LogError(e.Exception, "Telegram client exception occured");

        private void OnError(object sender, ReceiveErrorEventArgs e)
            => this._log.LogError("{ErrorCode} - {Error}", e.ApiRequestException.ErrorCode, e.ApiRequestException.Message);

        public void Stop()
        {
            lock (_clientLock)
                this.StopInternal();
        }

        private void StopInternal()
        {
            this._log.LogInformation("Stopping Telegram client");
            this.IsRunning = false;
            this.Client.StopReceiving();
        }

        private void DisposeInternalClient()
        {
            if (this.Client == null)
                return;
            try { this.Client.OnMessage -= OnMessage; } catch { }
            try { this.Client.OnReceiveError -= OnError; } catch { }
            try { this.Client.OnReceiveGeneralError -= OnGeneralError; } catch { }
            if (this.Client is IDisposable disposableClient)
                try { disposableClient?.Dispose(); } catch { }
            this.Client = null;
        }

        public void Dispose()
        {
            lock (_clientLock)
            {
                this.StopInternal();
                try { this._cts?.Cancel(); } catch { }
                try { this._cts?.Dispose(); } catch { }
                try { this._optionsChangeRegistration?.Dispose(); } catch { }
                this.DisposeInternalClient();
            }
        }
    }
}
