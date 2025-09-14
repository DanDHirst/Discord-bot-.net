using System.Text;
using System.Text.Json;
using DiscordBot.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace DiscordBot.Services;

public class TimerApiService
{
    private readonly AuthService _authService;
    private readonly ILogger<TimerApiService> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    public TimerApiService(AuthService authService, ILogger<TimerApiService> logger)
    {
        _authService = authService;
        _logger = logger;
        
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        };
    }

    public async Task<TimerResponse?> CreateTimerAsync(CreateTimerRequest request)
    {
        try
        {
            using var httpClient = await _authService.GetAuthenticatedHttpClientAsync();
            
            var json = JsonSerializer.Serialize(request, _jsonOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            _logger.LogInformation($"Sending timer request to API: {json}");
            
            var response = await httpClient.PostAsync("/api/timer", content);
            
            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                _logger.LogInformation($"Timer created successfully: {responseContent}");
                
                return JsonSerializer.Deserialize<TimerResponse>(responseContent, _jsonOptions);
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError($"Failed to create timer. Status: {response.StatusCode}, Error: {errorContent}");
                return null;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception occurred while creating timer");
            return null;
        }
    }

    public async Task<List<TimerResponse>> GetExpiredTimersAsync()
    {
        try
        {
            using var httpClient = await _authService.GetAuthenticatedHttpClientAsync();
            var response = await httpClient.GetAsync("/api/timer/expired");
            
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var expiredResponse = JsonSerializer.Deserialize<ExpiredTimersResponse>(content, _jsonOptions);
                return expiredResponse?.ExpiredTimers ?? new List<TimerResponse>();
            }
            else
            {
                _logger.LogError($"Failed to get expired timers. Status: {response.StatusCode}");
                return new List<TimerResponse>();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception occurred while getting expired timers");
            return new List<TimerResponse>();
        }
    }

    public async Task<bool> CompleteTimerAsync(int timerId)
    {
        try
        {
            using var httpClient = await _authService.GetAuthenticatedHttpClientAsync();
            var response = await httpClient.PostAsync($"/api/timer/{timerId}/complete", null);
            
            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation($"Timer {timerId} marked as completed");
                return true;
            }
            else
            {
                _logger.LogError($"Failed to complete timer {timerId}. Status: {response.StatusCode}");
                return false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Exception occurred while completing timer {timerId}");
            return false;
        }
    }
}
