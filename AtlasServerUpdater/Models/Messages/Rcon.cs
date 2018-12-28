namespace AtlasServerUpdater.Models.Messages
{
    /// <summary>
    /// Class Rcon.
    /// </summary>
    public class Rcon
    {
        /// <summary>
        /// Gets or sets the rcon update message.
        /// </summary>
        /// <value>The rcon update message.</value>
        public string RconUpdateMessage { get; set; }
        /// <summary>
        /// Gets or sets the rcon server uptime message.
        /// </summary>
        /// <value>The rcon server uptime message.</value>
        public string RconServerUptimeMessage { get; set; }
        /// <summary>
        /// Gets or sets the rcon server restarting message.
        /// </summary>
        /// <value>The rcon server restarting message.</value>
        public string RconServerRestartingMessage { get; set; }
        /// <summary>
        /// Gets or sets the rcon server not running.
        /// </summary>
        /// <value>The rcon server not running.</value>
        public string RconServerNotRunning { get; set; }
    }
}
