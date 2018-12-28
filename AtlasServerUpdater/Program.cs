using AtlasServerUpdater.Interfaces;
using AtlasServerUpdater.Models.Messages;
using AtlasServerUpdater.Models.Settings;
using AtlasServerUpdater.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.SystemConsole.Themes;
using System.Threading.Tasks;


namespace AtlasServerUpdater
{
    /// <summary>
    /// Class Program.
    /// </summary>
    class Program
    {
        /// <summary>
        /// Defines the entry point of the application.
        /// </summary>
        /// <param name="args">The arguments.</param>
        /// <returns>Task.</returns>
        public static async Task Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                .Enrich.FromLogContext()
                .WriteTo.Console(theme: AnsiConsoleTheme.Code)
                .WriteTo.RollingFile("log-{Date}.txt")
                .CreateLogger();

            IHostBuilder builder = new HostBuilder()
                .ConfigureAppConfiguration((hostingContext, config) =>
                {
                    config.AddJsonFile("updatersettings.json", false);
                    config.AddJsonFile("messages.json", false);
                    config.AddEnvironmentVariables();

                    if (args != null)
                    {
                        config.AddCommandLine(args);
                    }
                })
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddOptions();
                    services.Configure<ConsoleLifetimeOptions>(options => options.SuppressStatusMessages = true);
                    services.Configure<Settings>(hostContext.Configuration.GetSection("Settings"));
                    services.Configure<Messages>(hostContext.Configuration.GetSection("Messages"));

                    services.AddSingleton<ITwitchMessageService, TwitchMessageService>();
                    services.AddSingleton<IDiscordMessageService, DiscordMessageService>();
                    services.AddSingleton<ISteamCmdService, SteamCmdService>();
                    services.AddSingleton<IAutoRestartServerService, AutoRestartServerService>();
                    services.AddSingleton<IRconMessageService, RconMessageService>();
                    services.AddSingleton<IHostedService, UpdaterService>();
                })
                .UseSerilog();

            Log.Information("Starting up Console Host.");
            await builder.RunConsoleAsync();
        }
    }
}
