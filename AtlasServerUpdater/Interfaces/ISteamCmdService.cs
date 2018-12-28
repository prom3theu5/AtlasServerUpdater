using AtlasServerUpdater.Enums;
using System.Threading.Tasks;

namespace AtlasServerUpdater.Interfaces
{
    /// <summary>
    /// Interface ISteamCmdService
    /// </summary>
    public interface ISteamCmdService
    {
        /// <summary>
        /// Detects the update.
        /// </summary>
        /// <returns>Task&lt;System.ValueTuple&lt;System.Boolean, System.String&gt;&gt;.</returns>
        Task<(bool Result, string Version)> DetectUpdate();
        /// <summary>
        /// Kills the atlas.
        /// </summary>
        /// <returns>Task&lt;System.Boolean&gt;.</returns>
        Task<bool> KillAtlas();
        /// <summary>
        /// Installs the steam command.
        /// </summary>
        /// <returns>Task&lt;System.ValueTuple&lt;System.Boolean, System.Nullable&lt;FailureReasonEnum&gt;&gt;&gt;.</returns>
        Task<(bool InstallResult, FailureReasonEnum? Reason)> InstallSteamCmd();
        /// <summary>
        /// Starts the atlas server.
        /// </summary>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        bool StartAtlasServer();
        /// <summary>
        /// Determines whether [is steam command installed].
        /// </summary>
        /// <returns><c>true</c> if [is steam command installed]; otherwise, <c>false</c>.</returns>
        bool IsSteamCmdInstalled();
        /// <summary>
        /// Installs the and update atlas server.
        /// </summary>
        void InstallAndUpdateAtlasServer();
    }
}