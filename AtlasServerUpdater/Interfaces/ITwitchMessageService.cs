namespace AtlasServerUpdater.Interfaces
{
    /// <summary>
    /// Interface ITwitchMessageService
    /// </summary>
    public interface ITwitchMessageService
    {
        /// <summary>
        /// Sends the message.
        /// </summary>
        /// <param name="message">The message.</param>
        void SendMessage(string message);
    }
}