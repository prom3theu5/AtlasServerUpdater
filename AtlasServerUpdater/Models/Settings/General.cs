namespace AtlasServerUpdater.Models.Settings
{
    public class General
    {
        public bool ShouldRestartAtlasOnNotRunning { get; set; } = true;
        public int RestartServerAfterHours { get; set; }
    }
}
