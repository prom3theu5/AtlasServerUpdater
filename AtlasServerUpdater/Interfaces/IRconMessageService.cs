using System.Threading.Tasks;

namespace AtlasServerUpdater.Interfaces
{
    public interface IRconMessageService
    {
        Task SendMessage(string message);
    }
}