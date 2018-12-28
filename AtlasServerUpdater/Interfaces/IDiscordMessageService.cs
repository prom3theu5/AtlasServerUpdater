using System.Threading.Tasks;

namespace AtlasServerUpdater.Interfaces
{
    /// <summary>
    /// Interface IDiscordMessageService
    /// </summary>
    public interface IDiscordMessageService
    {
        /// <summary>
        /// Sends the message.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <returns>Task.</returns>
        Task SendMessage(string message);
    }
}