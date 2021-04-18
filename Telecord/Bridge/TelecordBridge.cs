using System;
using System.Collections.Generic;
using System.Linq;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TehGM.Telecord.Telegram;
using Telegram.Bot.Args;
using Telegram.Bot.Types.InputFiles;
using System.Threading;
using Telegram.Bot.Types;
using TelegramMessageType = Telegram.Bot.Types.Enums.MessageType;
using DiscordMessageType = Discord.MessageType;
using TelegramFile = Telegram.Bot.Types.File;
using File = System.IO.File;
using IDiscordClient = TehGM.Telecord.Discord.IDiscordClient;
using System.Threading.Tasks;
using Telegram.Bot.Types.Enums;

namespace TehGM.Telecord.Bridge.Services
{
    class TelecordBridge : IBridge, IDisposable
    {
        private readonly ILogger _log;
        private readonly IDiscordClient _discordClient;
        private readonly ITelegramClient _telegramClient;
        private readonly CancellationTokenSource _cts;

        private ILookup<ulong, BridgeLink> _discordBridgeLinks;
        private ILookup<long, BridgeLink> _telegramBridgeLinks;

        public TelecordBridge(IDiscordClient discordClient, ITelegramClient telegramClient, IOptionsMonitor<BridgeOptions> options, ILogger<TelecordBridge> log)
        {
            this._discordClient = discordClient;
            this._telegramClient = telegramClient;
            this._log = log;
            this._cts = new CancellationTokenSource();

            this.MapBridgeLinks(options.CurrentValue.Links);

            this._discordClient.MessageReceived += OnDiscordMessage;
            this._telegramClient.MessageReceived += OnTelegramMessage;
        }

        private async void OnTelegramMessage(object sender, MessageEventArgs e)
        {
            IEnumerable<BridgeLink> links = this._telegramBridgeLinks[e.Message.Chat.Id];
            if (!links.Any())
                return;

            // build message
            string username = e.Message.From.Username;
            string message = null;
            if (e.Message.Type == TelegramMessageType.ChatMembersAdded)
                message = $"***{username}*** *has joined the chat.";
            else if (e.Message.Type == TelegramMessageType.ChatMemberLeft)
                message = $"***{username}*** *has left the chat.";
            else if (e.Message.Type is TelegramMessageType.Text or TelegramMessageType.Photo or TelegramMessageType.Sticker)
            {
                message = $"***{username}*** *sent*:\r\n";
                if (!string.IsNullOrWhiteSpace(e.Message.ReplyToMessage?.Text))
                    message += $"> {e.Message.ReplyToMessage.Text}\r\n";
                if (!string.IsNullOrWhiteSpace(e.Message.Text))
                    message += e.Message.Text;
            }

            // send message
            CancellationToken cancellationToken = this._cts.Token;
            RequestOptions requestOptions = new RequestOptions() { CancelToken = cancellationToken };
            foreach (BridgeLink l in links)
            {
                SocketChannel channel = this._discordClient.Client.GetChannel(l.DiscordChannelID);
                if (channel is not SocketTextChannel textChannel)
                    continue;
                if (e.Message.Type == TelegramMessageType.Sticker)
                    await SendFileAsync(textChannel, e.Message.Sticker, message).ConfigureAwait(false);
                else
                    await textChannel.SendMessageAsync(message, options: requestOptions).ConfigureAwait(false);
                if (e.Message.Photo?.Any() == true)
                {
                    foreach (PhotoSize photo in e.Message.Photo)
                        await SendFileAsync(textChannel, photo, null).ConfigureAwait(false);
                }
            }

            async Task SendFileAsync(SocketTextChannel channel, FileBase file, string message)
            {
                this._log.LogTrace("Downloading Telegram file {ID}", file.FileId);
                TelegramFile downloadedFile = await _telegramClient.Client.GetFileAsync(e.Message.Sticker.FileId, cancellationToken).ConfigureAwait(false);
                await channel.SendFileAsync(downloadedFile.FilePath, message, options: requestOptions).ConfigureAwait(false);
                this._log.LogTrace("Deleting Telegram file {ID}: {Path}", file.FileId, downloadedFile.FilePath);
                File.Delete(downloadedFile.FilePath);
            }
        }

        private async void OnDiscordMessage(object sender, SocketMessage e)
        {
            IEnumerable<BridgeLink> links = this._discordBridgeLinks[e.Channel.Id];
            if (!links.Any())
                return;
            if (string.IsNullOrWhiteSpace(e.Content) && e.Attachments?.Any() != true)
                return;

            // build message
            SocketGuildUser guildUser = e.Author as SocketGuildUser;
            string username = $"{guildUser?.Nickname ?? e.Author.Username}#{e.Author.Discriminator}";
            string message = $"<i><b>{username}</b> sent</i>:";
            if (!string.IsNullOrWhiteSpace(e.Content))
                message += $"\r\n{e.Content}";

            // send message
            CancellationToken cancellationToken = this._cts.Token;
            foreach (BridgeLink l in links)
            {
                await this._telegramClient.Client.SendTextMessageAsync(l.TelegramChannelID, message, ParseMode.Html, cancellationToken: cancellationToken).ConfigureAwait(false);
                if (e.Attachments?.Any() == true)
                {
                    foreach (Attachment attachment in e.Attachments)
                        await this._telegramClient.Client.SendPhotoAsync(l.TelegramChannelID, new InputOnlineFile(attachment.ProxyUrl ?? attachment.Url), cancellationToken: cancellationToken).ConfigureAwait(false);
                }
            }
        }

        private void MapBridgeLinks(IEnumerable<BridgeLink> links)
        {
            this._log.LogDebug("Mapping {Count} bridge links", links.Count());
            this._discordBridgeLinks = links
                .Where(l => l.Direction is BridgeDirection.ToTelegram or BridgeDirection.Bidirectional)
                .ToLookup(l => l.DiscordChannelID, l => l);
            this._telegramBridgeLinks = links
                .Where(l => l.Direction is BridgeDirection.ToDiscord or BridgeDirection.Bidirectional)
                .ToLookup(l => l.TelegramChannelID, l => l);
        }

        public void Dispose()
        {
            try { this._cts?.Cancel(); } catch { }
            try { this._cts?.Dispose(); } catch { }
            try { this._discordClient.MessageReceived -= OnDiscordMessage; } catch { }
            try { this._telegramClient.MessageReceived -= OnTelegramMessage; } catch { }
        }
    }
}
