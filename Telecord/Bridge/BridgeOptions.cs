using System.Collections.Generic;
using Microsoft.Extensions.Options;

namespace TehGM.Telecord.Bridge
{
    public class BridgeOptions : IValidateOptions<BridgeOptions>
    {
        public IEnumerable<BridgeLink> Links { get; set; }

        public ValidateOptionsResult Validate(string name, BridgeOptions options)
        {
            foreach (BridgeLink link in options.Links)
            {
                if (link.DiscordChannelID == default || link.TelegramChannelID == default)
                    return ValidateOptionsResult.Fail($"Both {nameof(link.DiscordChannelID)} and {nameof(link.TelegramChannelID)} need to be configured for a bridge link");
            }
            return ValidateOptionsResult.Success;
        }
    }
}
