namespace AtlasServerUpdater.Interfaces
{
    /// <summary>
    /// Interface IAutoRestartServerService
    /// </summary>
    public interface IAutoRestartServerService
    {
        /// <summary>
        /// Starts the timer.
        /// </summary>
        void StartTimer();
        /// <summary>
        /// Stops the timer.
        /// </summary>
        void StopTimer();
    }
}
