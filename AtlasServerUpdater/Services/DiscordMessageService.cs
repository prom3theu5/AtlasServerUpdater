using AtlasServerUpdater.Interfaces;
using AtlasServerUpdater.Models.Settings;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Threading.Tasks;


namespace AtlasServerUpdater.Services
{
    public class DiscordMessageService : IDiscordMessageService
    {
        private readonly Settings _settings;
        private readonly DiscordSocketClient _client;
        private readonly ILogger<DiscordMessageService> _logger;

        public DiscordMessageService(ILogger<DiscordMessageService> logger, IOptionsSnapshot<Settings> settings)
        {
            _logger = logger;
            _settings = settings.Value;
            if (!_settings.Update.AnnounceDiscord)
            {
                _logger.LogInformation("Discord Messaging is Disabled in Application Settings");
                return;
            }

            if (string.IsNullOrWhiteSpace(_settings.Discord.DiscordToken) || _settings.Discord.ChannelId == 0)
            {
                throw new InvalidOperationException(
                    "In order to start the Discord service, you must supply a bot token, and a channelId for notifications to appear in.");
            }

            _logger.LogInformation("{this} > Starting Discord Connection", nameof(TwitchMessageService));

            _client = new DiscordSocketClient();
            _client.Log += LogAsync;
            _client.Ready += ReadyAsync;

            Task.Run(async () =>
            {
                await _client.LoginAsync(TokenType.Bot, _settings.Discord.DiscordToken);
                await _client.StartAsync();
                await Task.Delay(-1);
            });

        }

        private Task ReadyAsync()
        {
            _logger.LogInformation("{this} > Connected to discord as {user}", nameof(DiscordMessageService), _client.CurrentUser);
            return Task.CompletedTask;
        }

        private Task LogAsync(LogMessage arg)
        {
            _logger.LogInformation("{this} > Discord Log: {message}", nameof(DiscordMessageService), arg.Message);
            return Task.CompletedTask;
        }

        public async Task SendMessage(string message)
        {
            SocketChannel channel = _client.GetChannel(_settings.Discord.ChannelId);

            if (channel is null)
            {
                _logger.LogError("Channel was not found to send discord notification to: {channel}", _settings.Discord.ChannelId);
                return;
            }

            IMessageChannel sendChannel = channel as IMessageChannel;
            await sendChannel.SendMessageAsync(message);

        }
    }
}
