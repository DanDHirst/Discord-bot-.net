using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace DiscordBot.Services;

public class AuthService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<AuthService> _logger;
    private readonly IConfiguration _configuration;
    private readonly JsonSerializerOptions _jsonOptions;
    
    private string? _currentToken;
    private DateTime _tokenExpiry = DateTime.MinValue;
    private readonly object _tokenLock = new();

    public AuthService(HttpClient httpClient, IConfiguration configuration, ILogger<AuthService> logger)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _logger = logger;
        
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
    }

    public async Task<string?> GetValidTokenAsync()
    {
        lock (_tokenLock)
        {
            // If we have a valid token that expires in more than 5 minutes, return it
            if (!string.IsNullOrEmpty(_currentToken) && _tokenExpiry > DateTime.UtcNow.AddMinutes(5))
            {
                return _currentToken;
            }
        }

        // Token is expired or about to expire, get a new one
        return await RefreshTokenAsync();
    }

    private async Task<string?> RefreshTokenAsync()
    {
        try
        {
            var apiKey = _configuration["API:ApiKey"];
            if (string.IsNullOrEmpty(apiKey))
            {
                _logger.LogError("API:ApiKey not configured in appsettings.json");
                return null;
            }

            var apiBaseUrl = _configuration["API:BaseUrl"] ?? "https://localhost:7001";
            var tokenRequest = new { ApiKey = apiKey };
            var json = JsonSerializer.Serialize(tokenRequest, _jsonOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            _logger.LogInformation("Requesting new JWT token from API");
            
            var response = await _httpClient.PostAsync($"{apiBaseUrl}/api/auth/token", content);
            
            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var tokenResponse = JsonSerializer.Deserialize<TokenResponse>(responseContent, _jsonOptions);
                
                if (tokenResponse != null && !string.IsNullOrEmpty(tokenResponse.Token))
                {
                    lock (_tokenLock)
                    {
                        _currentToken = tokenResponse.Token;
                        _tokenExpiry = tokenResponse.ExpiresAt;
                    }
                    
                    _logger.LogInformation($"Successfully obtained JWT token, expires at: {tokenResponse.ExpiresAt}");
                    return tokenResponse.Token;
                }
                else
                {
                    _logger.LogError("Token response was null or empty");
                    return null;
                }
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError($"Failed to get JWT token. Status: {response.StatusCode}, Error: {errorContent}");
                return null;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception occurred while getting JWT token");
            return null;
        }
    }

    public async Task<HttpClient> GetAuthenticatedHttpClientAsync()
    {
        var token = await GetValidTokenAsync();
        if (string.IsNullOrEmpty(token))
        {
            throw new InvalidOperationException("Unable to obtain valid JWT token");
        }

        // Create a new HttpClient instance or clone the existing one
        var authenticatedClient = new HttpClient();
        
        // Copy base configuration
        var apiBaseUrl = _configuration["API:BaseUrl"] ?? "https://localhost:7001";
        authenticatedClient.BaseAddress = new Uri(apiBaseUrl);
        
        // Add authorization header
        authenticatedClient.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        return authenticatedClient;
    }

    private class TokenResponse
    {
        public string Token { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
        public string TokenType { get; set; } = string.Empty;
    }
}
