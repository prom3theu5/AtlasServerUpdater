namespace AtlasServerUpdater.Models.Messages
{
    /// <summary>
    /// Class Discord.
    /// </summary>
    public class Discord
    {
        /// <summary>
        /// Gets or sets the discord update message.
        /// </summary>
        /// <value>The discord update message.</value>
        public string DiscordUpdateMessage { get; set; }
        /// <summary>
        /// Gets or sets the discord server uptime message.
        /// </summary>
        /// <value>The discord server uptime message.</value>
        public string DiscordServerUptimeMessage { get; set; }
        /// <summary>
        /// Gets or sets the discord server restarting message.
        /// </summary>
        /// <value>The discord server restarting message.</value>
        public string DiscordServerRestartingMessage { get; set; }
        /// <summary>
        /// Gets or sets the discord server not running.
        /// </summary>
        /// <value>The discord server not running.</value>
        public string DiscordServerNotRunning { get; set; }
    }
}
