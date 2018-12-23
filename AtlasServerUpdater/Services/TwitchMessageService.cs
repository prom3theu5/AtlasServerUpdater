using AtlasServerUpdater.Interfaces;
using AtlasServerUpdater.Models.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using TwitchLib.Client;
using TwitchLib.Client.Enums;
using TwitchLib.Client.Models;

namespace AtlasServerUpdater.Services
{
    public class TwitchMessageService : ITwitchMessageService
    {
        private readonly Settings _settings;
        private readonly TwitchClient _client;
        private readonly ILogger<TwitchMessageService> _logger;

        public TwitchMessageService(ILogger<TwitchMessageService> logger, IOptionsSnapshot<Settings> settings)
        {
            _logger = logger;
            _settings = settings.Value;
            if (!_settings.Update.AnnounceTwitch)
            {
                _logger.LogInformation("Twitch Messaging is Disabled in Application Settings");
                return;
            }

            if (string.IsNullOrWhiteSpace(_settings.Twitch.Username) ||
                string.IsNullOrWhiteSpace(_settings.Twitch.OAuth))
            {
                throw new InvalidOperationException("In order to start the twitch service, you must supply a username and oauth token.");
            }

            _client = new TwitchClient(protocol: ClientProtocol.TCP);
            _logger.LogInformation("{this} > Starting Twitch Connection", nameof(TwitchMessageService));

            ConnectionCredentials connectionCredentials = new ConnectionCredentials(_settings.Twitch.Username, _settings.Twitch.OAuth);
            _client.Initialize(connectionCredentials, _settings.Twitch.Channel);
            _client.OnConnected += _client_OnConnected;
            _client.OnJoinedChannel += _client_OnJoinedChannel;
            _client.OnDisconnected += _client_OnDisconnected;

            _client.Connect();

        }

        public void SendMessage(string message)
        {
            if (!_settings.Update.AnnounceTwitch) return;

            if (!_client.IsConnected)
            {
                _logger.LogError("Error Sending Twitch Message. Client is Not Connected");
            }

            _client.SendMessage(_settings.Twitch.Channel, message);
        }

        private void _client_OnDisconnected(object sender, TwitchLib.Communication.Events.OnDisconnectedEventArgs e)
        {
            _logger.LogInformation("Disconnected from Twitch");
        }

        private void _client_OnJoinedChannel(object sender, TwitchLib.Client.Events.OnJoinedChannelArgs e)
        {
            _logger.LogInformation("Joined Twitch Channel: '{channel}'", _settings.Twitch.Channel);
        }

        private void _client_OnConnected(object sender, TwitchLib.Client.Events.OnConnectedArgs e)
        {
            _logger.LogInformation("Connected to Twitch as '{username}'", _settings.Twitch.Username);
        }
    }
}
