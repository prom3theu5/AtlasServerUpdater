namespace AtlasServerUpdater.Models.Messages
{
    /// <summary>
    /// Class Messages.
    /// </summary>
    public class Messages
    {
        /// <summary>
        /// Gets or sets the discord.
        /// </summary>
        /// <value>The discord.</value>
        public Discord Discord { get; set; }
        /// <summary>
        /// Gets or sets the twitch.
        /// </summary>
        /// <value>The twitch.</value>
        public Twitch Twitch { get; set; }
        /// <summary>
        /// Gets or sets the rcon.
        /// </summary>
        /// <value>The rcon.</value>
        public Rcon RCON { get; set; }
    }
}
