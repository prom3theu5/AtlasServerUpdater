using AtlasServerUpdater.Enums;
using AtlasServerUpdater.Interfaces;
using AtlasServerUpdater.Models.Messages;
using AtlasServerUpdater.Models.Settings;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using Twitch = AtlasServerUpdater.Models.Messages.Twitch;

namespace AtlasServerUpdater.Services
{
    /// <summary>
    /// Class UpdaterService.
    /// Implements the <see cref="Microsoft.Extensions.Hosting.IHostedService" />
    /// </summary>
    /// <seealso cref="Microsoft.Extensions.Hosting.IHostedService" />
    public class UpdaterService : IHostedService
    {
        /// <summary>
        /// The announce before
        /// </summary>
        private const string AnnounceBefore = "@announcebefore";

        #region Private Properties
        /// <summary>
        /// The logger
        /// </summary>
        private readonly ILogger<UpdaterService> _logger;
        /// <summary>
        /// The settings
        /// </summary>
        private readonly Settings _settings;
        /// <summary>
        /// The autorestart service
        /// </summary>
        private readonly IAutoRestartServerService _autorestartService;
        /// <summary>
        /// The twitch message service
        /// </summary>
        private readonly ITwitchMessageService _twitchMessageService;
        /// <summary>
        /// The discord message service
        /// </summary>
        private readonly IDiscordMessageService _discordMessageService;
        /// <summary>
        /// The rcon message service
        /// </summary>
        private readonly IRconMessageService _rconMessageService;
        /// <summary>
        /// The steam command service
        /// </summary>
        private readonly ISteamCmdService _steamCmdService;
        /// <summary>
        /// The update timer
        /// </summary>
        private readonly System.Timers.Timer _updateTimer;
        /// <summary>
        /// The check game running timer
        /// </summary>
        private System.Timers.Timer _checkGameRunningTimer;
        /// <summary>
        /// The twitch messages
        /// </summary>
        private readonly Twitch _twitchMessages;
        /// <summary>
        /// The discord messages
        /// </summary>
        private readonly Models.Messages.Discord _discordMessages;
        /// <summary>
        /// The rcon messages
        /// </summary>
        private readonly Models.Messages.Rcon _rconMessages;
        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="UpdaterService"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="settings">The settings.</param>
        /// <param name="twitchMessageService">The twitch message service.</param>
        /// <param name="discordMessageService">The discord message service.</param>
        /// <param name="steamCmdService">The steam command service.</param>
        /// <param name="messageTemplates">The message templates.</param>
        /// <param name="autorestartService">The autorestart service.</param>
        /// <param name="rconMessageService">The rcon message service.</param>
        public UpdaterService(
            ILogger<UpdaterService> logger,
            IOptionsSnapshot<Settings> settings,
            ITwitchMessageService twitchMessageService,
            IDiscordMessageService discordMessageService,
            ISteamCmdService steamCmdService,
            IOptionsSnapshot<Messages> messageTemplates,
            IAutoRestartServerService autorestartService,
            IRconMessageService rconMessageService)
        {
            _logger = logger;
            _settings = settings.Value;
            _autorestartService = autorestartService;
            _twitchMessageService = twitchMessageService;
            _discordMessageService = discordMessageService;
            _rconMessageService = rconMessageService;
            _steamCmdService = steamCmdService;
            _twitchMessages = messageTemplates.Value.Twitch;
            _discordMessages = messageTemplates.Value.Discord;
            _rconMessages = messageTemplates.Value.RCON;
            _logger.LogInformation("Updater Service has Started");

            if (_settings.Update.ShouldInstallSteamCmdIfMissing)
                InstallSteamCMD();

            if (_settings.Update.ShouldInstallAtlasServerIfMissing)
            {
                Process process = Process.GetProcesses().FirstOrDefault(c => c.ProcessName.Contains(_settings.Atlas.ServerProcessName));
                if (process is null)
                    _steamCmdService.InstallAndUpdateAtlasServer();
            }


            _updateTimer = new System.Timers.Timer(_settings.Update.UpdateCheckInterval * 1000 * 60)
            {
                AutoReset = true,
                Enabled = false
            };

            if (_settings.General.ShouldRestartAtlasOnNotRunning)
            {
                SetupServerProcessMonitor();
            }

            if (_settings.General.RestartServerAfterHours != 0)
            {
                _autorestartService.StartTimer();
            }
        }

        /// <summary>
        /// Setups the server process monitor.
        /// </summary>
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

        /// <summary>
        /// Handles the Elapsed event of the _checkGameRunningTimer control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="ElapsedEventArgs"/> instance containing the event data.</param>
        private void _checkGameRunningTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            Process process = Process.GetProcesses().FirstOrDefault(c => c.ProcessName.Contains(_settings.Atlas.ServerProcessName));

            if (process is null)
            {
                _logger.LogCritical("Server Process not found to be running. Will try to start now.");
                if (_steamCmdService.StartAtlasServer())
                {
                    _logger.LogInformation("The Atlas Server has been started Successfully!");
                }
            }
        }

        /// <summary>
        /// Installs the steam command.
        /// </summary>
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

        /// <summary>
        /// Triggered when the application host is ready to start the service.
        /// </summary>
        /// <param name="cancellationToken">Indicates that the start process has been aborted.</param>
        /// <returns>Task.</returns>
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

        /// <summary>
        /// Triggered when the application host is performing a graceful shutdown.
        /// </summary>
        /// <param name="cancellationToken">Indicates that the shutdown process should no longer be graceful.</param>
        /// <returns>Task.</returns>
        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Updater Service has Stopped");
            return Task.CompletedTask;
        }

        /// <summary>
        /// Timers the elapsed.
        /// </summary>
        /// <param name="e">The <see cref="System.Timers.ElapsedEventArgs"/> instance containing the event data.</param>
        /// <returns>Task.</returns>
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

                if (_settings.Update.AnnounceRCon)
                {
                    string rconMessage = _rconMessages.RconUpdateMessage.Replace("@version", $"{updateCheck.Version}").Replace(AnnounceBefore, $"{_settings.Update.AnnounceMinutesBefore}{MinutesPluralisation()}");

                    await _rconMessageService.SendMessage(rconMessage);
                }

                await Task.Delay(TimeSpan.FromMinutes(_settings.Update.AnnounceMinutesBefore));

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

        /// <summary>
        /// Minuteses the pluralisation.
        /// </summary>
        /// <returns>System.String.</returns>
        private string MinutesPluralisation()
        {
            return (_settings.Update.AnnounceMinutesBefore == 1 ? "Minute" : "Minutes");
        }
    }
}
