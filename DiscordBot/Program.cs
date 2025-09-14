using Discord;
using Discord.WebSocket;
using DiscordBot.Models;
using DiscordBot.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;

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
                
                // Configure HttpClient with SSL certificate bypass for development
                services.AddHttpClient<TimerApiService>(client =>
                {
                    client.Timeout = TimeSpan.FromSeconds(30);
                }).ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
                {
                    ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
                });
                
                services.AddHttpClient<BlockedUserService>((serviceProvider, client) =>
                {
                    var config = serviceProvider.GetRequiredService<IConfiguration>();
                    var apiBaseUrl = config["API:BaseUrl"] ?? "https://localhost:7001";
                    client.BaseAddress = new Uri(apiBaseUrl);
                    client.Timeout = TimeSpan.FromSeconds(30);
                }).ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
                {
                    ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
                });
                
                services.AddSingleton<TimerApiService>();
                services.AddSingleton<BlockedUserService>();
                services.AddSingleton<BotService>();
                services.AddHostedService<TimerNotificationService>();
            })
            .ConfigureLogging(logging =>
            {
                logging.ClearProviders();
                logging.AddConsole();
            })
            .Build();

        // Start the host (this will start all hosted services including TimerNotificationService)
        await host.StartAsync();
        
        // Start the bot
        var botService = host.Services.GetRequiredService<BotService>();
        await botService.StartAsync();

        // Keep the application running
        await host.WaitForShutdownAsync();
    }
}

public class BotService
{
    private readonly DiscordSocketClient _client;
    private readonly IConfiguration _configuration;
    private readonly ILogger<BotService> _logger;
    private readonly TimerApiService _timerApiService;
    private readonly BlockedUserService _blockedUserService;

    public BotService(DiscordSocketClient client, IConfiguration configuration, ILogger<BotService> logger, TimerApiService timerApiService, BlockedUserService blockedUserService)
    {
        _client = client;
        _configuration = configuration;
        _logger = logger;
        _timerApiService = timerApiService;
        _blockedUserService = blockedUserService;

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

        // Debug logging to see what we're receiving
        _logger.LogInformation($"Message received from {message.Author.Username}: '{message.Content}' (Length: {message.Content.Length})");
        _logger.LogInformation($"Channel: {message.Channel.Name}, Author ID: {message.Author.Id}");

        var content = message.Content.Trim();

        if (string.IsNullOrEmpty(content))
        {
            _logger.LogWarning("Received empty message content - check Message Content Intent!");
            return;
        }

        // Check if user is blocked
        var userId = message.Author.Id.ToString();
        var isBlocked = await _blockedUserService.IsUserBlockedAsync(userId);
        
        if (isBlocked)
        {
            _logger.LogInformation($"Blocked user {message.Author.Username} ({userId}) attempted to use bot");
            await message.Channel.SendMessageAsync($"🚫 {message.Author.Mention}, you are currently blocked from using bot commands.");
            return;
        }

        // Handle PING command
        if (content.ToUpperInvariant() == "PING")
        {
            _logger.LogInformation($"Received PING from {message.Author.Username}");
            await message.Channel.SendMessageAsync("PONG");
            return;
        }

        // Handle TIMER command - matches "TIMER <number>" or "TIMER <number> <message>"
        // Updated regex to catch negative numbers and better error handling
        var timerMatch = Regex.Match(content, @"^TIMER\s+(-?\d+)(?:\s+(.+))?$", RegexOptions.IgnoreCase);
        if (timerMatch.Success)
        {
            await HandleTimerCommand(message, timerMatch);
            return;
        }
        
        // Check if user tried to use TIMER but with invalid format
        if (content.ToUpperInvariant().StartsWith("TIMER"))
        {
            await message.Channel.SendMessageAsync("⚠️ Invalid timer format!\n" +
                                                   "Usage: `TIMER <minutes>` or `TIMER <minutes> <reminder message>`\n" +
                                                   "Examples:\n" +
                                                   "• `TIMER 5` - Set a 5-minute timer\n" +
                                                   "• `TIMER 10 Check the oven` - Timer with reminder");
            return;
        }

        // Log other messages for debugging
        _logger.LogInformation($"Received non-command message: '{content}'");
    }

    private async Task HandleTimerCommand(SocketMessage message, Match timerMatch)
    {
        try
        {
            var durationString = timerMatch.Groups[1].Value;
            var customMessage = timerMatch.Groups.Count > 2 ? timerMatch.Groups[2].Value : null;

            if (!int.TryParse(durationString, out var duration) || duration < 1 || duration > 1440)
            {
                await message.Channel.SendMessageAsync("⚠️ Invalid timer format!\n" +
                                                       "Usage: `TIMER <minutes>` or `TIMER <minutes> <reminder message>`\n" +
                                                       "Examples:\n" +
                                                       "• `TIMER 5` - Set a 5-minute timer\n" +
                                                       "• `TIMER 10 Check the oven` - Timer with reminder");
                return;
            }

            _logger.LogInformation($"Creating {duration}-minute timer for {message.Author.Username}");

            var timerRequest = new CreateTimerRequest
            {
                UserId = message.Author.Id.ToString(),
                Username = message.Author.Username,
                ChannelId = message.Channel.Id,
                DurationMinutes = duration,
                Message = customMessage
            };

            var createdTimer = await _timerApiService.CreateTimerAsync(timerRequest);

            if (createdTimer != null)
            {
                var confirmMessage = $"✅ Timer set for {duration} minute{(duration == 1 ? "" : "s")}! I'll notify you when it's done.";
                if (!string.IsNullOrEmpty(customMessage))
                {
                    confirmMessage += $"\n💭 Reminder: {customMessage}";
                }
                confirmMessage += $"\n🕐 Expires at: <t:{((DateTimeOffset)createdTimer.ExpiresAt).ToUnixTimeSeconds()}:F>";

                await message.Channel.SendMessageAsync(confirmMessage);
                _logger.LogInformation($"Successfully created timer {createdTimer.Id} for user {message.Author.Username}");
            }
            else
            {
                await message.Channel.SendMessageAsync("❌ Sorry, I couldn't create your timer. Please try again later.");
                _logger.LogError($"Failed to create timer for user {message.Author.Username}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error handling timer command from {message.Author.Username}");
            await message.Channel.SendMessageAsync("❌ An error occurred while setting up your timer. Please try again.");
        }
    }
}
