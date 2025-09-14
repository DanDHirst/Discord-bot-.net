using System.Text.Json.Serialization;

namespace DiscordBot.Models;

public class CreateTimerRequest
{
    [JsonPropertyName("userId")]
    public string UserId { get; set; } = string.Empty;
    
    [JsonPropertyName("username")]
    public string Username { get; set; } = string.Empty;
    
    [JsonPropertyName("channelId")]
    public ulong ChannelId { get; set; }
    
    [JsonPropertyName("durationMinutes")]
    public int DurationMinutes { get; set; }
    
    [JsonPropertyName("message")]
    public string? Message { get; set; }
}

public class TimerResponse
{
    [JsonPropertyName("id")]
    public int Id { get; set; }
    
    [JsonPropertyName("userId")]
    public string UserId { get; set; } = string.Empty;
    
    [JsonPropertyName("username")]
    public string Username { get; set; } = string.Empty;
    
    [JsonPropertyName("channelId")]
    public ulong ChannelId { get; set; }
    
    [JsonPropertyName("durationMinutes")]
    public int DurationMinutes { get; set; }
    
    [JsonPropertyName("createdAt")]
    public DateTime CreatedAt { get; set; }
    
    [JsonPropertyName("expiresAt")]
    public DateTime ExpiresAt { get; set; }
    
    [JsonPropertyName("isCompleted")]
    public bool IsCompleted { get; set; }
    
    [JsonPropertyName("completedAt")]
    public DateTime? CompletedAt { get; set; }
    
    [JsonPropertyName("message")]
    public string? Message { get; set; }
}

public class ExpiredTimersResponse
{
    [JsonPropertyName("expiredTimers")]
    public List<TimerResponse> ExpiredTimers { get; set; } = new();
}
