using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace DiscordBot;

class Program
{
    static async Task Main(string[] args)
    {
        // Build configuration
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();

        // Create host
        var host = Host.CreateDefaultBuilder(args)
            .ConfigureServices((context, services) =>
            {
                services.AddSingleton(configuration);
                services.AddSingleton<DiscordSocketClient>(provider =>
                {
                    var config = new DiscordSocketConfig
                    {
                        GatewayIntents = GatewayIntents.AllUnprivileged | GatewayIntents.MessageContent,
                        LogLevel = LogSeverity.Info
                    };
                    return new DiscordSocketClient(config);
                });
                services.AddSingleton<BotService>();
            })
            .ConfigureLogging(logging =>
            {
                logging.ClearProviders();
                logging.AddConsole();
            })
            .Build();

        // Start the bot
        var botService = host.Services.GetRequiredService<BotService>();
        await botService.StartAsync();

        // Keep the application running
        await Task.Delay(-1);
    }
}

public class BotService
{
    private readonly DiscordSocketClient _client;
    private readonly IConfiguration _configuration;
    private readonly ILogger<BotService> _logger;

    public BotService(DiscordSocketClient client, IConfiguration configuration, ILogger<BotService> logger)
    {
        _client = client;
        _configuration = configuration;
        _logger = logger;

        // Configure Discord client events
        _client.Log += LogAsync;
        _client.Ready += ReadyAsync;
        _client.MessageReceived += MessageReceivedAsync;
    }

    public async Task StartAsync()
    {
        var token = _configuration["Discord:Token"];
        
        if (string.IsNullOrEmpty(token))
        {
            _logger.LogError("Discord token is not configured. Please add your token to appsettings.json");
            return;
        }

        await _client.LoginAsync(TokenType.Bot, token);
        await _client.StartAsync();
    }

    private Task LogAsync(LogMessage log)
    {
        _logger.LogInformation($"{log.Source}: {log.Message}");
        return Task.CompletedTask;
    }

    private Task ReadyAsync()
    {
        _logger.LogInformation($"{_client.CurrentUser} is connected!");
        return Task.CompletedTask;
    }

    private async Task MessageReceivedAsync(SocketMessage message)
    {
        // Don't process the message if it was sent by a bot
        if (message.Author.IsBot)
            return;

        var content = message.Content.Trim().ToUpperInvariant();

        // Handle PING command
        if (content == "PING")
        {
            _logger.LogInformation($"Received PING from {message.Author.Username}");
            await message.Channel.SendMessageAsync("PONG");
        }
    }
}
