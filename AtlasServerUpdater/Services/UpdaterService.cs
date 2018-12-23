using AtlasServerUpdater.Enums;
using AtlasServerUpdater.Interfaces;
using AtlasServerUpdater.Models.Messages;
using AtlasServerUpdater.Models.Settings;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using Twitch = AtlasServerUpdater.Models.Messages.Twitch;

namespace AtlasServerUpdater.Services
{
    public class UpdaterService : IHostedService
    {
        private const string AnnounceBefore = "@announcebefore";

        #region Private Properties
        private readonly ILogger<UpdaterService> _logger;
        private readonly Settings _settings;
        private readonly ITwitchMessageService _twitchMessageService;
        private readonly IDiscordMessageService _discordMessageService;
        private readonly ISteamCmdService _steamCmdService;
        private readonly System.Timers.Timer _updateTimer;
        private System.Timers.Timer _checkGameRunningTimer;
        private readonly Twitch _twitchMessages;
        private readonly Models.Messages.Discord _discordMessages;
        #endregion

        public UpdaterService(
            ILogger<UpdaterService> logger,
            IOptionsSnapshot<Settings> settings,
            ITwitchMessageService twitchMessageService,
            IDiscordMessageService discordMessageService,
            ISteamCmdService steamCmdService,
            IOptionsSnapshot<Messages> messageTemplates)
        {
            _logger = logger;
            _settings = settings.Value;
            _twitchMessageService = twitchMessageService;
            _discordMessageService = discordMessageService;
            _steamCmdService = steamCmdService;
            _twitchMessages = messageTemplates.Value.Twitch;
            _discordMessages = messageTemplates.Value.Discord;
            _logger.LogInformation("Updater Service has Started");

            if (_settings.Update.ShouldInstallSteamCmdIfMissing)
                InstallSteamCMD();

            if (_settings.Update.ShouldInstallAtlasServerIfMissing)
                _steamCmdService.InstallAndUpdateAtlasServer();

            _updateTimer = new System.Timers.Timer(_settings.Update.UpdateCheckInterval * 1000 * 60)
            {
                AutoReset = true,
                Enabled = false
            };

            if (_settings.General.ShouldRestartAtlasOnNotRunning)
            {
                SetupServerProcessMonitor();
            }
        }

        private void SetupServerProcessMonitor()
        {
            _checkGameRunningTimer = new System.Timers.Timer(30000)
            {
                AutoReset = true,
                Enabled = true,
            };

            _checkGameRunningTimer.Elapsed += _checkGameRunningTimer_Elapsed;
            _checkGameRunningTimer.Start();
        }

        private void _checkGameRunningTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            _logger.LogInformation("Options configured to monitor server running state. Checking if server process is running");
            Process process = Process.GetProcesses().FirstOrDefault(c => c.ProcessName.Contains(_settings.Atlas.Executable));

            if (process is null)
            {
                _logger.LogCritical("Server Process not found to be running. Will try to start now.");
                _steamCmdService.StartAtlasServer();
            }
        }

        private void InstallSteamCMD()
        {
            _logger.LogInformation("Checking to see if SteamCmd Exists");
            (bool InstallResult, FailureReasonEnum? Reason) installSteamCmdResult = _steamCmdService.InstallSteamCmd().GetAwaiter().GetResult();
            if (!installSteamCmdResult.InstallResult)
            {
                switch (installSteamCmdResult.Reason)
                {
                    case FailureReasonEnum.AlreadyExists:
                        _logger.LogInformation("SteamCMD is already installed. Skipping Installation..");
                        break;
                    case FailureReasonEnum.NotFound:
                    case FailureReasonEnum.Unknown:
                        _logger.LogError("SteamCMD is flagged as to be installed, but the install failed. We cannot continue, and must abort the update check.");
                        return;
                }
            }
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _updateTimer.Elapsed += async (s, e) => await _timer_Elapsed(e);
            _updateTimer.Enabled = true;
            _updateTimer.Start();

            _logger.LogInformation("Resuming Normal Scheduled Checks.");
            _logger.LogInformation("Check for Updates Every {interval} minutes", _settings.Update.UpdateCheckInterval);
            if (_settings.General.ShouldRestartAtlasOnNotRunning)
            {
                _logger.LogInformation("Monitoring Server Process running every {interval} seconds", 30);
            }

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Updater Service has Stopped");
            return Task.CompletedTask;
        }

        private async Task _timer_Elapsed(System.Timers.ElapsedEventArgs e)
        {
            _checkGameRunningTimer?.Dispose();

            if (!_steamCmdService.IsSteamCmdInstalled())
            {
                _logger.LogError("Could not find SteamCMD - we need it to perform the update. Aborting");
                return;
            }

            _logger.LogInformation("Performing Update Check");

            (bool Result, string Version) updateCheck = await _steamCmdService.DetectUpdate();
            if (updateCheck.Result)
            {
                if (_settings.Update.AnnounceTwitch)
                {
                    string twitchMessage = _twitchMessages.TwitchUpdateMessage.Replace("@version", $"{updateCheck.Version}").Replace(AnnounceBefore, $"{_settings.Update.AnnounceMinutesBefore}{MinutesPluralisation()}");

                    _twitchMessageService.SendMessage(twitchMessage);
                }

                if (_settings.Update.AnnounceTwitch)
                {
                    string discordMessage = _discordMessages.DiscordUpdateMessage.Replace("@version", $"{updateCheck.Version}").Replace(AnnounceBefore, $"{_settings.Update.AnnounceMinutesBefore}{MinutesPluralisation()}");

                    await _discordMessageService.SendMessage(discordMessage);
                }

                _logger.LogInformation("Updating...");
                bool updateResult = await _steamCmdService.KillAtlas();

                if (!updateResult)
                {
                    _logger.LogInformation("Update process has failed to stop the running server.");
                    return;
                }

                _steamCmdService.InstallAndUpdateAtlasServer();

                bool result = _steamCmdService.StartAtlasServer();
                if (result)
                {
                    if (_settings.Update.AnnounceTwitch)
                    {
                        _twitchMessageService.SendMessage(_twitchMessages.TwitchServerRestartingMessage);
                    }

                    if (_settings.Update.AnnounceTwitch)
                    {
                        await _discordMessageService.SendMessage(_discordMessages.DiscordServerRestartingMessage);
                    }
                }

                _logger.LogInformation("Server Has Started Back Up.");
            }

            if (_settings.General.ShouldRestartAtlasOnNotRunning)
                SetupServerProcessMonitor();
        }

        private string MinutesPluralisation()
        {
            return (_settings.Update.AnnounceMinutesBefore == 1 ? "Minute" : "Minutes");
        }
    }
}
