namespace AtlasServerUpdater.Models.Settings
{
    /// <summary>
    /// Class Discord.
    /// </summary>
    public class Discord
    {
        /// <summary>
        /// Gets or sets the discord token.
        /// </summary>
        /// <value>The discord token.</value>
        public string DiscordToken { get; set; }
        /// <summary>
        /// Gets or sets the channel identifier.
        /// </summary>
        /// <value>The channel identifier.</value>
        public ulong ChannelId { get; set; }
    }
}
