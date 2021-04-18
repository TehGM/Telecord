namespace TehGM.Telecord.Bridge
{
    public class BridgeLink
    {
        public BridgeDirection Direction { get; set; } = BridgeDirection.Bidirectional;
        public ulong DiscordChannelID { get; set; }
        public long TelegramChannelID { get; set; }
    }
}
