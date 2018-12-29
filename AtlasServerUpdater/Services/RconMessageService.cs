using AtlasServerUpdater.Interfaces;
using AtlasServerUpdater.Models.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using RCON = AtlasServerUpdater.Models.Settings.RCON;

namespace AtlasServerUpdater.Services
{
    /// <summary>
    /// Class RconMessageService.
    /// Implements the <see cref="AtlasServerUpdater.Interfaces.IRconMessageService" />
    /// </summary>
    /// <seealso cref="AtlasServerUpdater.Interfaces.IRconMessageService" />
    public class RconMessageService : IRconMessageService
    {
        /// <summary>
        /// The settings
        /// </summary>
        private readonly Settings _settings;
        /// <summary>
        /// The logger
        /// </summary>
        private readonly ILogger<RconMessageService> _logger;
        /// <summary>
        /// The clients
        /// </summary>
        private readonly List<CoreRCON.RCON> _clients;
        /// <summary>
        /// The rcon settings
        /// </summary>
        private readonly RCON _rconSettings;

        /// <summary>
        /// Initializes a new instance of the <see cref="RconMessageService"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="settings">The settings.</param>
        /// <exception cref="InvalidOperationException">In order to start the Rcon Service, you must supply credentials in the format 'IP,QueryPort,Password'</exception>
        public RconMessageService(ILogger<RconMessageService> logger, IOptionsSnapshot<Settings> settings)
        {
            _logger = logger;
            _settings = settings.Value;
            _rconSettings = settings.Value.RCON;
            _clients = new List<CoreRCON.RCON>();
            if (!_settings.Update.AnnounceRCon)
            {
                _logger.LogInformation("RCON Messaging is Disabled in Application Settings");
                return;
            }

            if (!_rconSettings.Servers.Any())
            {
                throw new InvalidOperationException(
                    "In order to start the Rcon Service, you must supply credentials in the format 'IP,QueryPort,Password'");
            }

            foreach (string server in _rconSettings.Servers)
            {
                string[] data = server.Split(',');
                IPAddress.TryParse(data[0], out IPAddress ip);
                ushort.TryParse(data[1], out ushort port);

                if (ip == null || port == 0 || data[2] == null)
                    continue;

                _clients.Add(new CoreRCON.RCON(ip, port, data[2]));
            }
        }

        /// <summary>
        /// Sends the message.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <returns>Task.</returns>
        public async Task SendMessage(string message)
        {
            foreach (CoreRCON.RCON client in _clients)
            {
                await client.SendCommandAsync($"broadcast \"{message}\"");
            }
        }
    }
}
