namespace AtlasServerUpdater.Models.Messages
{
    public class Messages
    {
        public Discord Discord { get; set; }
        public Twitch Twitch { get; set; }
        public int AnnounceIntervalInMinutes { get; set; }
    }
}
