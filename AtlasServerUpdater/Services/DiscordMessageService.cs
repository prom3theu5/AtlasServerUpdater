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
    /// <summary>
    /// Class DiscordMessageService.
    /// Implements the <see cref="AtlasServerUpdater.Interfaces.IDiscordMessageService" />
    /// </summary>
    /// <seealso cref="AtlasServerUpdater.Interfaces.IDiscordMessageService" />
    public class DiscordMessageService : IDiscordMessageService
    {
        /// <summary>
        /// The settings
        /// </summary>
        private readonly Settings _settings;
        /// <summary>
        /// The client
        /// </summary>
        private readonly DiscordSocketClient _client;
        /// <summary>
        /// The logger
        /// </summary>
        private readonly ILogger<DiscordMessageService> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="DiscordMessageService"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="settings">The settings.</param>
        /// <exception cref="InvalidOperationException">In order to start the Discord service, you must supply a bot token, and a channelId for notifications to appear in.</exception>
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

        /// <summary>
        /// Readies the asynchronous.
        /// </summary>
        /// <returns>Task.</returns>
        private Task ReadyAsync()
        {
            _logger.LogInformation("{this} > Connected to discord as {user}", nameof(DiscordMessageService), _client.CurrentUser);
            return Task.CompletedTask;
        }

        /// <summary>
        /// Logs the asynchronous.
        /// </summary>
        /// <param name="arg">The argument.</param>
        /// <returns>Task.</returns>
        private Task LogAsync(LogMessage arg)
        {
            _logger.LogInformation("{this} > Discord Log: {message}", nameof(DiscordMessageService), arg.Message);
            return Task.CompletedTask;
        }

        /// <summary>
        /// Sends the message.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <returns>Task.</returns>
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
