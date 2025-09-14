using Discord.WebSocket;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace DiscordBot.Services;

public class TimerNotificationService : BackgroundService
{
    private readonly TimerApiService _timerApiService;
    private readonly DiscordSocketClient _discordClient;
    private readonly ILogger<TimerNotificationService> _logger;
    private readonly TimeSpan _checkInterval = TimeSpan.FromSeconds(30); // Check every 30 seconds

    public TimerNotificationService(
        TimerApiService timerApiService,
        DiscordSocketClient discordClient,
        ILogger<TimerNotificationService> logger)
    {
        _timerApiService = timerApiService;
        _discordClient = discordClient;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Timer notification service started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                // Wait for Discord client to be ready
                if (_discordClient.ConnectionState == Discord.ConnectionState.Connected)
                {
                    await CheckExpiredTimers();
                }
                else
                {
                    _logger.LogDebug("Discord client not connected, skipping timer check");
                }

                await Task.Delay(_checkInterval, stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in timer notification service");
                await Task.Delay(_checkInterval, stoppingToken);
            }
        }
    }

    private async Task CheckExpiredTimers()
    {
        try
        {
            _logger.LogDebug($"Checking for expired timers at {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC");
            var expiredTimers = await _timerApiService.GetExpiredTimersAsync();

            if (expiredTimers.Any())
            {
                _logger.LogInformation($"Found {expiredTimers.Count} expired timer(s)");

                foreach (var timer in expiredTimers)
                {
                    _logger.LogInformation($"Processing expired timer {timer.Id} for user {timer.Username}");
                    await SendTimerNotification(timer);
                    await _timerApiService.CompleteTimerAsync(timer.Id);
                }
            }
            else
            {
                _logger.LogDebug("No expired timers found");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking expired timers");
        }
    }

    private async Task SendTimerNotification(DiscordBot.Models.TimerResponse timer)
    {
        try
        {
            var channel = _discordClient.GetChannel(timer.ChannelId) as SocketTextChannel;
            
            if (channel == null)
            {
                _logger.LogWarning($"Could not find channel {timer.ChannelId} for timer {timer.Id}");
                return;
            }

            var message = $"⏰ <@{timer.UserId}> Your {timer.DurationMinutes}-minute timer is up!";
            
            if (!string.IsNullOrEmpty(timer.Message))
            {
                message += $"\n💭 Reminder: {timer.Message}";
            }

            await channel.SendMessageAsync(message);
            _logger.LogInformation($"Sent timer notification for timer {timer.Id} to user {timer.Username}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error sending timer notification for timer {timer.Id}");
        }
    }
}
