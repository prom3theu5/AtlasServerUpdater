using AtlasServerUpdater.Enums;
using System.Threading.Tasks;

namespace AtlasServerUpdater.Interfaces
{
    public interface ISteamCmdService
    {
        Task<(bool Result, string Version)> DetectUpdate();
        Task<bool> KillAtlas();
        Task<(bool InstallResult, FailureReasonEnum? Reason)> InstallSteamCmd();
        bool StartAtlasServer();
        bool IsSteamCmdInstalled();
        void InstallAndUpdateAtlasServer();
    }
}