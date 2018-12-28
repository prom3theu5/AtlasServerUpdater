namespace AtlasServerUpdater.Interfaces
{
    public interface IAutoRestartServerService
    {
        void StartTimer();
        void StopTimer();
    }
}
