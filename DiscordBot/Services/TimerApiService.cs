using System.Text;
using System.Text.Json;
using DiscordBot.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace DiscordBot.Services;

public class TimerApiService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<TimerApiService> _logger;
    private readonly string _apiBaseUrl;
    private readonly JsonSerializerOptions _jsonOptions;

    public TimerApiService(HttpClient httpClient, IConfiguration configuration, ILogger<TimerApiService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        _apiBaseUrl = configuration["API:BaseUrl"] ?? "https://localhost:7001";
        
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        };
        
        // Configure HTTP client to ignore SSL certificate errors in development
        _httpClient.BaseAddress = new Uri(_apiBaseUrl);
    }

    public async Task<TimerResponse?> CreateTimerAsync(CreateTimerRequest request)
    {
        try
        {
            var json = JsonSerializer.Serialize(request, _jsonOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            _logger.LogInformation($"Sending timer request to API: {json}");
            
            var response = await _httpClient.PostAsync("/api/timer", content);
            
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
            var response = await _httpClient.GetAsync("/api/timer/expired");
            
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
            var response = await _httpClient.PostAsync($"/api/timer/{timerId}/complete", null);
            
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
