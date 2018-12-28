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
    public class AutoRestartServerService : IAutoRestartServerService
    {
        private readonly Settings _settings;
        private readonly ILogger<AutoRestartServerService> _logger;
        private readonly ISteamCmdService _steamCmdService;
        private readonly IDiscordMessageService _discordMessageService;
        private readonly IRconMessageService _rconMessageService;
        private readonly ITwitchMessageService _twitchMessageService;
        private readonly Models.Messages.Twitch _twitchMessages;
        private readonly Models.Messages.Discord _discordMessages;
        private readonly Models.Messages.Rcon _rconMessages;
        private System.Timers.Timer _timer;

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

        public void StopTimer()
        {
            _timer?.Stop();
            _timer?.Dispose();
        }

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
