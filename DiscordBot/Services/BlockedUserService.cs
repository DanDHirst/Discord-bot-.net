using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace DiscordBot.Services;

public class BlockedUserService
{
    private readonly AuthService _authService;
    private readonly ILogger<BlockedUserService> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    public BlockedUserService(AuthService authService, ILogger<BlockedUserService> logger)
    {
        _authService = authService;
        _logger = logger;
        
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
    }

    public async Task<bool> IsUserBlockedAsync(string userId)
    {
        try
        {
            using var httpClient = await _authService.GetAuthenticatedHttpClientAsync();
            var requestUrl = $"/api/blockedusers/check/{userId}";
                
            _logger.LogDebug($"Making request to: {requestUrl}");
            
            var response = await httpClient.GetAsync(requestUrl);
            
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
            _logger.LogError(ex, $"Exception occurred while checking if user {userId} is blocked");
            return false; // Default to not blocked if exception occurs
        }
    }
}
