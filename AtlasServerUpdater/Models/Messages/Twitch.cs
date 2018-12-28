namespace AtlasServerUpdater.Models.Messages
{
    /// <summary>
    /// Class Twitch.
    /// </summary>
    public class Twitch
    {
        /// <summary>
        /// Gets or sets the twitch update message.
        /// </summary>
        /// <value>The twitch update message.</value>
        public string TwitchUpdateMessage { get; set; }
        /// <summary>
        /// Gets or sets the twitch server uptime message.
        /// </summary>
        /// <value>The twitch server uptime message.</value>
        public string TwitchServerUptimeMessage { get; set; }
        /// <summary>
        /// Gets or sets the twitch server restarting message.
        /// </summary>
        /// <value>The twitch server restarting message.</value>
        public string TwitchServerRestartingMessage { get; set; }
        /// <summary>
        /// Gets or sets the twitch server not running.
        /// </summary>
        /// <value>The twitch server not running.</value>
        public string TwitchServerNotRunning { get; set; }
    }
}
