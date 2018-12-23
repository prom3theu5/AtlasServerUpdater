namespace AtlasServerUpdater.Models.Settings
{
    public class Settings
    {
        public Twitch Twitch { get; set; }
        public Discord Discord { get; set; }
        public Atlas Atlas { get; set; }
        public Update Update { get; set; }
        public General General { get; set; }
    }
}
