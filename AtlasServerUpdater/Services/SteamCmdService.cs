using AtlasServerUpdater.Enums;
using AtlasServerUpdater.Interfaces;
using AtlasServerUpdater.Models.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace AtlasServerUpdater.Services
{
    /// <summary>
    /// Class SteamCmdService.
    /// Implements the <see cref="AtlasServerUpdater.Interfaces.ISteamCmdService" />
    /// </summary>
    /// <seealso cref="AtlasServerUpdater.Interfaces.ISteamCmdService" />
    public class SteamCmdService : ISteamCmdService
    {
        /// <summary>
        /// The logger
        /// </summary>
        private readonly ILogger<SteamCmdService> _logger;
        /// <summary>
        /// The settings
        /// </summary>
        private readonly Settings _settings;
        /// <summary>
        /// The atlas server appid
        /// </summary>
        private const string AtlasServerAppid = "1006030";
        /// <summary>
        /// The steam command executable
        /// </summary>
        private const string SteamCmdExe = "steamcmd.exe";
        /// <summary>
        /// The steam command download zip URL
        /// </summary>
        private const string SteamCmdDownloadZipUrl = "https://steamcdn-a.akamaihd.net/client/installer/steamcmd.zip";
        /// <summary>
        /// The steam command zip filename
        /// </summary>
        private const string SteamCmdZipFilename = "steamcmd.zip";

        /// <summary>
        /// Initializes a new instance of the <see cref="SteamCmdService"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="settings">The settings.</param>
        public SteamCmdService(ILogger<SteamCmdService> logger, IOptionsSnapshot<Settings> settings)
        {
            _logger = logger;
            _settings = settings.Value;
        }

        /// <summary>
        /// Determines whether [is steam command installed].
        /// </summary>
        /// <returns><c>true</c> if [is steam command installed]; otherwise, <c>false</c>.</returns>
        public bool IsSteamCmdInstalled()
        {
            if (Directory.Exists(_settings.Update.SteamCmdPath))
            {
                if (File.Exists(Path.Combine(_settings.Update.SteamCmdPath, SteamCmdExe)))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Installs the steam command.
        /// </summary>
        /// <returns>Task&lt;System.ValueTuple&lt;System.Boolean, System.Nullable&lt;FailureReasonEnum&gt;&gt;&gt;.</returns>
        public async Task<(bool InstallResult, FailureReasonEnum? Reason)> InstallSteamCmd()
        {
            try
            {
                if (IsSteamCmdInstalled()) return (false, FailureReasonEnum.AlreadyExists);

                _logger.LogInformation("SteamCMD Missing. Downloading Now");
                Directory.CreateDirectory(_settings.Update.SteamCmdPath);
                using (WebClient wc = new WebClient())
                {
                    wc.DownloadFile(new Uri(SteamCmdDownloadZipUrl), Path.Combine(_settings.Update.SteamCmdPath, SteamCmdZipFilename));
                    _logger.LogInformation("Extracting SteamCMD Zip");
                    ZipFile.ExtractToDirectory(Path.Combine(_settings.Update.SteamCmdPath, SteamCmdZipFilename), _settings.Update.SteamCmdPath);
                    await Task.Run(() =>
                    {
                        ProcessStartInfo processStartInfo = new ProcessStartInfo
                        {
                            FileName = $"{_settings.Update.SteamCmdPath}{SteamCmdExe}",
                            Arguments = "+quit",
                            RedirectStandardOutput = false,
                            UseShellExecute = false
                        };
                        Process.Start(processStartInfo)?.WaitForExit();
                    });
                    _logger.LogInformation("SteamCMD installed successfully");
                    File.Delete(Path.Combine(_settings.Update.SteamCmdPath, SteamCmdZipFilename));
                    return (true, null);
                }
            }
            catch (Exception e)
            {
                _logger.LogError("Error downloading SteamCMD: {Exception}", e.Message);
                return (false, FailureReasonEnum.Unknown);
            }
        }

        /// <summary>
        /// Installs the and update atlas server.
        /// </summary>
        public void InstallAndUpdateAtlasServer()
        {
            ProcessStartInfo processStartInfo = new ProcessStartInfo
            {
                FileName = $"{_settings.Update.SteamCmdPath}{SteamCmdExe}",
                Arguments =
                    $"+@ShutdownOnFailedCommand +@NoPromptForPassword 1 +nSubscribedAutoDownloadMaxSimultaneous 32 +@cMaxContentServersToRequest 16 +@cMaxInitialDownloadSources 1 +@fMinDataRateToAttemptTwoConnectionsMbps 0.01 +@fDownloadRateImprovementToAddAnotherConnection 0.01 +login anonymous +force_install_dir {_settings.Atlas.FolderPath} +app_update {AtlasServerAppid} +quit",
                RedirectStandardOutput = false,
                UseShellExecute = false
            };
            Process.Start(processStartInfo)?.WaitForExit();
        }

        /// <summary>
        /// Detects the update.
        /// </summary>
        /// <returns>Task&lt;System.ValueTuple&lt;System.Boolean, System.String&gt;&gt;.</returns>
        public Task<(bool Result, string Version)> DetectUpdate()
        {
            return Task.Run(() =>
            {
                try
                {
                    if (_settings.Update.InstalledBuild == 0)
                    {
                        if (string.IsNullOrWhiteSpace(_settings.Atlas.FolderPath))
                        {
                            _logger.LogError("You must set your Installation Folder path in the config.json file");
                            return (false, null);
                        }

                        _logger.LogInformation(
                            "Current Atlas Dedicated Server Build Version not stored in Settings, Trying to get it now from your Application Manifest file");

                        string installedBuild = File.ReadAllLines(Path.Combine(_settings.Atlas.FolderPath, "steamapps",
                                $"appmanifest_{AtlasServerAppid}.acf"))
                            ?.FirstOrDefault(c => c.Contains("buildid"))
                            ?.Split('\t', '\t')
                            ?.LastOrDefault()
                            ?.Trim()
                            ?.Replace("\"", "");
                        if (installedBuild == null)
                            _logger.LogError(
                                $"Couldn't get installed Atlas Dedicated Server Build Version from your appmanifest_{AtlasServerAppid}.acf file. Please set this manually in config.json");
                        else
                        {
                            _settings.Update.InstalledBuild = Convert.ToInt32(installedBuild);
                            _logger.LogInformation(
                                "Atlas Dedicated Server Build Version has been detected and set as {buildversion}",
                                installedBuild);
                            return (false, null);
                        }
                    }

                    // Clear SteamCMD App Cache so correct Live build is Pulled!
                    // App needs to be admin, or have write permissions on the steamcmd directory.
                    string cache = Path.Combine(_settings.Update.SteamCmdPath, "appcache");
                    if (Directory.Exists(cache))
                    {
                        Directory.Delete(cache, true);
                    }

                    ProcessStartInfo processStartInfo = new ProcessStartInfo
                    {
                        FileName = $"{_settings.Update.SteamCmdPath}{SteamCmdExe}",
                        Arguments =
                            $"+login anonymous +app_info_update 1 +app_info_print {AtlasServerAppid} +app_info_print {AtlasServerAppid} +app_info_print {AtlasServerAppid} +quit",
                        RedirectStandardOutput = true,
                        UseShellExecute = false
                    };
                    Process process = Process.Start(processStartInfo);
                    string[] output = process.StandardOutput.ReadToEnd().Split('\r', '\n');
                    process.WaitForExit();
                    string steamVersionString = output.FirstOrDefault(c => c.Contains("buildid"));
                    if (steamVersionString == null)
                    {
                        _logger.LogError("Steam Version was Not Detected");
                        return (false, null);
                    }

                    int steamVersion = Convert.ToInt32(steamVersionString.Split(new char[] { '\t', '\t' })
                        ?.LastOrDefault()
                        ?.Trim()
                        ?.Replace("\"", ""));
                    if (steamVersion <= _settings.Update.InstalledBuild)
                    {
                        _logger.LogInformation(
                            "Installed Version is the same or greater than the steam version. No Update Needed!");
                        return (false, null);
                    }

                    _logger.LogInformation(
                        "Detected SteamVersion as: {steamversion}. Your version is: {localVersion} An Update is required.",
                        steamVersion, _settings.Update.InstalledBuild.ToString());

                    _settings.Update.InstalledBuild = Convert.ToInt32(steamVersion);
                    return (true, _settings.Update.InstalledBuild.ToString());
                }
                catch (Exception e)
                {
                    _logger.LogError("Exception occured in Detecting Update: {exception}", e.Message);
                    return (false, null);
                }
            });
        }

        /// <summary>
        /// Starts the atlas server.
        /// </summary>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        public bool StartAtlasServer()
        {
            try
            {
                foreach (string batchFile in _settings.Atlas.BatchFiles)
                {
                    ProcessStartInfo processStartInfo = new ProcessStartInfo
                    {
                        FileName = $"{batchFile}",
                        RedirectStandardOutput = false,
                        UseShellExecute = false
                    };
                    Process.Start(processStartInfo);
                }


                Process processRunning = Process.GetProcesses().FirstOrDefault(c => c.ProcessName.Contains(_settings.Atlas.ServerProcessName));
                return processRunning != null;
            }
            catch (Exception e)
            {
                _logger.LogError("Failed to start the Atlas Server: {error}", e.Message);
                return false;
            }
        }

        /// <summary>
        /// Kills the atlas.
        /// </summary>
        /// <returns>Task&lt;System.Boolean&gt;.</returns>
        public async Task<bool> KillAtlas()
        {
            try
            {
                List<Process> processes = Process.GetProcesses().Where(c => c.ProcessName.Contains(_settings.Atlas.ServerProcessName)).ToList();
                if (processes.Any())
                {
                    foreach (Process process in processes)
                    {
                        process?.Kill();
                    }
                    // Wait 30 seconds for a clean shutdown
                    await Task.Delay(30 * 1000);
                }
                return true;
            }
            catch (Exception e)
            {
                _logger.LogError("Error updating server: {error}", e.Message);
                return false;
            }

        }
    }
}
