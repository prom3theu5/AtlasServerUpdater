namespace AtlasServerUpdater.Models.Settings
{
    /// <summary>
    /// Class Settings.
    /// </summary>
    public class Settings
    {
        /// <summary>
        /// Gets or sets the twitch.
        /// </summary>
        /// <value>The twitch.</value>
        public Twitch Twitch { get; set; }
        /// <summary>
        /// Gets or sets the discord.
        /// </summary>
        /// <value>The discord.</value>
        public Discord Discord { get; set; }
        /// <summary>
        /// Gets or sets the atlas.
        /// </summary>
        /// <value>The atlas.</value>
        public Atlas Atlas { get; set; }
        /// <summary>
        /// Gets or sets the update.
        /// </summary>
        /// <value>The update.</value>
        public Update Update { get; set; }
        /// <summary>
        /// Gets or sets the general.
        /// </summary>
        /// <value>The general.</value>
        public General General { get; set; }
        /// <summary>
        /// Gets or sets the rcon.
        /// </summary>
        /// <value>The rcon.</value>
        public RCON RCON { get; set; }
    }
}
