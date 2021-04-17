using System;
using System.Threading.Tasks;
using Discord.WebSocket;

namespace TehGM.Telecord.Discord
{
    public interface IDiscordClient
    {
        DiscordSocketClient Client { get; }
        event EventHandler<SocketMessage> MessageReceived;
        Task StartClientAsync();
        Task StopClientAsync();
    }
}
