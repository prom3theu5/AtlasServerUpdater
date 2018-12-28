namespace AtlasServerUpdater.Models.Settings
{
    /// <summary>
    /// Class Update.
    /// </summary>
    public class Update
    {
        /// <summary>
        /// Gets or sets a value indicating whether [should install steam command if missing].
        /// </summary>
        /// <value><c>true</c> if [should install steam command if missing]; otherwise, <c>false</c>.</value>
        public bool ShouldInstallSteamCmdIfMissing { get; set; } = true;
        /// <summary>
        /// Gets or sets a value indicating whether [should install atlas server if missing].
        /// </summary>
        /// <value><c>true</c> if [should install atlas server if missing]; otherwise, <c>false</c>.</value>
        public bool ShouldInstallAtlasServerIfMissing { get; set; } = false;
        /// <summary>
        /// Gets or sets the steam command path.
        /// </summary>
        /// <value>The steam command path.</value>
        public string SteamCmdPath { get; set; }
        /// <summary>
        /// Gets or sets a value indicating whether [announce discord].
        /// </summary>
        /// <value><c>true</c> if [announce discord]; otherwise, <c>false</c>.</value>
        public bool AnnounceDiscord { get; set; } = false;
        /// <summary>
        /// Gets or sets a value indicating whether [announce r con].
        /// </summary>
        /// <value><c>true</c> if [announce r con]; otherwise, <c>false</c>.</value>
        public bool AnnounceRCon { get; set; } = false;
        /// <summary>
        /// Gets or sets a value indicating whether [announce twitch].
        /// </summary>
        /// <value><c>true</c> if [announce twitch]; otherwise, <c>false</c>.</value>
        public bool AnnounceTwitch { get; set; } = false;
        /// <summary>
        /// Gets or sets the announce minutes before.
        /// </summary>
        /// <value>The announce minutes before.</value>
        public int AnnounceMinutesBefore { get; set; } = 5;
        /// <summary>
        /// Gets or sets the update check interval.
        /// </summary>
        /// <value>The update check interval.</value>
        public int UpdateCheckInterval { get; set; } = 5;
        /// <summary>
        /// Gets or sets the installed build.
        /// </summary>
        /// <value>The installed build.</value>
        public int InstalledBuild { get; set; }
        /// <summary>
        /// Gets or sets a value indicating whether [update on launch].
        /// </summary>
        /// <value><c>true</c> if [update on launch]; otherwise, <c>false</c>.</value>
        public bool UpdateOnLaunch { get; set; } = false;
    }
}
