namespace AtlasServerUpdater.Models.Settings
{
    /// <summary>
    /// Class General.
    /// </summary>
    public class General
    {
        /// <summary>
        /// Gets or sets a value indicating whether [should restart atlas on not running].
        /// </summary>
        /// <value><c>true</c> if [should restart atlas on not running]; otherwise, <c>false</c>.</value>
        public bool ShouldRestartAtlasOnNotRunning { get; set; } = true;
        /// <summary>
        /// Gets or sets the restart server after hours.
        /// </summary>
        /// <value>The restart server after hours.</value>
        public int RestartServerAfterHours { get; set; }
    }
}
