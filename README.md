# Telecord - a Telegram and Discord bridge bot
A simple and configurable .NET 5 bot for bridging messages between Telegram and Discord channels.


## Configuration
### Tokens
The bot needs bot tokens for both Discord and Telegram. You can create them on [Discord Developer Portal](https://discord.com/developers/applications) and by messaging `@BotFather` on Telegram.  
One tokens are generated, insert them into a new `appsecrets.json` file (recommended) or existing [`appsettings.json`](Telecord/appsettings.json) (insecure). The example appsecrets file can be seen here: [appsecrets-example.json](Telecord/appsecrets-example.json).

### Telegram privacy mode
You need to contact `@BotFather` to disable privacy mode for your bot if you want to bridge messages from Telegram to Discord.

### Bridge Links
You need to configure your bridge links. Bridge configurations are JSON objects inside `Links` array inside of `Bridges` category. See [appsettings.json](Telecord/appsettings.json) for example.

`DiscordChannelID` and `TelegramChannelID` are mandatory, and represent Discord Channel ID, and Telegram Chat ID respectively.  
`Direction` sets what direction the link will function. `ToTelegram` bridges Discord -> Telegram, `ToDiscord` bridges Telegram -> Discord, and `Bidirectional` bridges both ways. This value is optional - if not provided, will default to `Bidirectional`.


## Usage
### Requirements
An installed [.NET 5 SDK](https://dotnet.microsoft.com/download/dotnet/5.0) is required for building. Running requires Runtime, which is also included in SDK.

### Building
Run `dotnet build` and `dotnet publish` commands in the repository root folder.

### Running
Once compiled and published, run using `dotnet Telecord.dll` command in the published files folder.


## License
Copyright (c) 2021 TehGM

Licensed under [GNU Affero General Public License v3.0](LICENSE) (GNU AGPL-3.0).