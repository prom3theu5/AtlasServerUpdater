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
    public class RconMessageService : IRconMessageService
    {
        private readonly Settings _settings;
        private readonly ILogger<RconMessageService> _logger;
        private readonly List<CoreRCON.RCON> _clients;
        private readonly RCON _rconSettings;

        public RconMessageService(ILogger<RconMessageService> logger, IOptionsSnapshot<Settings> settings)
        {
            _logger = logger;
            _settings = settings.Value;
            _rconSettings = settings.Value.RCON;
            _clients = new List<CoreRCON.RCON>();
            if (!_settings.Update.AnnounceDiscord)
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

        public async Task SendMessage(string message)
        {
            foreach (CoreRCON.RCON client in _clients)
            {
                await client.SendCommandAsync($"broadcast \"{message}\"");
            }
        }
    }
}
