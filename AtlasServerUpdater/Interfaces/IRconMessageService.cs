using System.Threading.Tasks;

namespace AtlasServerUpdater.Interfaces
{
    /// <summary>
    /// Interface IRconMessageService
    /// </summary>
    public interface IRconMessageService
    {
        /// <summary>
        /// Sends the message.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <returns>Task.</returns>
        Task SendMessage(string message);
    }
}