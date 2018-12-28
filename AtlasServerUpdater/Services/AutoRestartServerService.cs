using AtlasServerUpdater.Interfaces;
using AtlasServerUpdater.Models.Messages;
using AtlasServerUpdater.Models.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Threading.Tasks;
using System.Timers;

namespace AtlasServerUpdater.Services
{
    /// <summary>
    /// Class AutoRestartServerService.
    /// Implements the <see cref="AtlasServerUpdater.Interfaces.IAutoRestartServerService" />
    /// </summary>
    /// <seealso cref="AtlasServerUpdater.Interfaces.IAutoRestartServerService" />
    public class AutoRestartServerService : IAutoRestartServerService
    {
        /// <summary>
        /// The settings
        /// </summary>
        private readonly Settings _settings;
        /// <summary>
        /// The logger
        /// </summary>
        private readonly ILogger<AutoRestartServerService> _logger;
        /// <summary>
        /// The steam command service
        /// </summary>
        private readonly ISteamCmdService _steamCmdService;
        /// <summary>
        /// The discord message service
        /// </summary>
        private readonly IDiscordMessageService _discordMessageService;
        /// <summary>
        /// The rcon message service
        /// </summary>
        private readonly IRconMessageService _rconMessageService;
        /// <summary>
        /// The twitch message service
        /// </summary>
        private readonly ITwitchMessageService _twitchMessageService;
        /// <summary>
        /// The twitch messages
        /// </summary>
        private readonly Models.Messages.Twitch _twitchMessages;
        /// <summary>
        /// The discord messages
        /// </summary>
        private readonly Models.Messages.Discord _discordMessages;
        /// <summary>
        /// The rcon messages
        /// </summary>
        private readonly Models.Messages.Rcon _rconMessages;
        /// <summary>
        /// The timer
        /// </summary>
        private System.Timers.Timer _timer;

        /// <summary>
        /// Initializes a new instance of the <see cref="AutoRestartServerService"/> class.
        /// </summary>
        /// <param name="discordMessageService">The discord message service.</param>
        /// <param name="twitchMessageService">The twitch message service.</param>
        /// <param name="settings">The settings.</param>
        /// <param name="messageTemplates">The message templates.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="steamCmdService">The steam command service.</param>
        /// <param name="rconMessageService">The rcon message service.</param>
        public AutoRestartServerService(IDiscordMessageService discordMessageService,
            ITwitchMessageService twitchMessageService,
            IOptionsSnapshot<Settings> settings,
            IOptionsSnapshot<Messages> messageTemplates,
            ILogger<AutoRestartServerService> logger,
            ISteamCmdService steamCmdService,
            IRconMessageService rconMessageService)
        {
            _settings = settings.Value;
            _logger = logger;
            _steamCmdService = steamCmdService;
            _discordMessageService = discordMessageService;
            _rconMessageService = rconMessageService;
            _twitchMessageService = twitchMessageService;
            _twitchMessages = messageTemplates.Value.Twitch;
            _discordMessages = messageTemplates.Value.Discord;
            _rconMessages = messageTemplates.Value.RCON;
        }

        /// <summary>
        /// Starts the timer.
        /// </summary>
        public void StartTimer()
        {

            _timer = new Timer(TimeSpan.FromHours(_settings.General.RestartServerAfterHours).TotalMilliseconds)
            {
                AutoReset = true,
                Enabled = true
            };

            _timer.Elapsed += async (s, e) => await _timer_Elapsed(e);

            _timer.Start();

            _logger.LogInformation("Server Will automatically restart every {hours} Hours.",
                _settings.General.RestartServerAfterHours);
        }

        /// <summary>
        /// Stops the timer.
        /// </summary>
        public void StopTimer()
        {
            _timer?.Stop();
            _timer?.Dispose();
        }

        /// <summary>
        /// Timers the elapsed.
        /// </summary>
        /// <param name="e">The <see cref="ElapsedEventArgs"/> instance containing the event data.</param>
        /// <returns>Task.</returns>
        private async Task _timer_Elapsed(ElapsedEventArgs e)
        {
            await Task.Run(async () =>
            {
                if (_settings.Update.AnnounceTwitch)
                {
                    _twitchMessageService.SendMessage(_twitchMessages.TwitchServerUptimeMessage.Replace("@hours", _settings.General.RestartServerAfterHours.ToString()));
                }

                if (_settings.Update.AnnounceTwitch)
                {
                    await _discordMessageService.SendMessage(_discordMessages.DiscordServerUptimeMessage.Replace("@hours", _settings.General.RestartServerAfterHours.ToString()));
                }

                if (_settings.Update.AnnounceRCon)
                {
                    await _rconMessageService.SendMessage(_rconMessages.RconServerUptimeMessage.Replace("@hours", _settings.General.RestartServerAfterHours.ToString()));
                }

                await Task.Delay(TimeSpan.FromMinutes(5));

                await _steamCmdService.KillAtlas();

                if (_settings.Update.AnnounceTwitch)
                {
                    _twitchMessageService.SendMessage(_twitchMessages.TwitchServerRestartingMessage);
                }

                if (_settings.Update.AnnounceTwitch)
                {
                    await _discordMessageService.SendMessage(_discordMessages.DiscordServerRestartingMessage);
                }

                _steamCmdService.StartAtlasServer();
            });
        }
    }
}
