using System.Threading.Tasks;

namespace AtlasServerUpdater.Interfaces
{
    public interface IDiscordMessageService
    {
        Task SendMessage(string message);
    }
}