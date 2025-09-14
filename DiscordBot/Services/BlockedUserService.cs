using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace DiscordBot.Services;

public class BlockedUserService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<BlockedUserService> _logger;
    private readonly JsonSerializerOptions _jsonOptions;
    private readonly string _apiBaseUrl;

    public BlockedUserService(HttpClient httpClient, IConfiguration configuration, ILogger<BlockedUserService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        _apiBaseUrl = configuration["API:BaseUrl"] ?? "https://localhost:7001";
        
        // Ensure BaseAddress is set
        if (_httpClient.BaseAddress == null)
        {
            _httpClient.BaseAddress = new Uri(_apiBaseUrl);
            _logger.LogInformation($"Set BaseAddress to: {_apiBaseUrl}");
        }
        else
        {
            _logger.LogInformation($"BaseAddress already set to: {_httpClient.BaseAddress}");
        }
        
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
    }

    public async Task<bool> IsUserBlockedAsync(string userId)
    {
        try
        {
            // Use absolute URL as fallback if BaseAddress issues persist
            var requestUrl = _httpClient.BaseAddress != null 
                ? $"/api/blockedusers/check/{userId}" 
                : $"{_apiBaseUrl}/api/blockedusers/check/{userId}";
                
            _logger.LogDebug($"Making request to: {requestUrl}");
            
            var response = await _httpClient.GetAsync(requestUrl);
            
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var isBlocked = JsonSerializer.Deserialize<bool>(content, _jsonOptions);
                
                _logger.LogInformation($"User {userId} blocked status: {isBlocked}");
                return isBlocked;
            }
            else
            {
                _logger.LogError($"Failed to check blocked status for user {userId}. Status: {response.StatusCode}");
                return false; // Default to not blocked if API call fails
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Exception occurred while checking if user {userId} is blocked. URL: {_httpClient.BaseAddress}");
            return false; // Default to not blocked if exception occurs
        }
    }
}
