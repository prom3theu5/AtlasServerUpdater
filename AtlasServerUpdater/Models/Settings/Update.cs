namespace AtlasServerUpdater.Models.Settings
{
    public class Update
    {
        public bool ShouldInstallSteamCmdIfMissing { get; set; } = true;
        public bool ShouldInstallAtlasServerIfMissing { get; set; } = false;
        public string SteamCmdPath { get; set; }
        public bool AnnounceDiscord { get; set; } = false;
        public bool AnnounceTwitch { get; set; } = false;
        public int AnnounceMinutesBefore { get; set; } = 5;
        public int UpdateCheckInterval { get; set; } = 5;
        public int InstalledBuild { get; set; }
        public bool UpdateOnLaunch { get; set; } = false;
    }
}
