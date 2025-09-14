using System.ComponentModel.DataAnnotations;

namespace API.Models;

public class CreateTimerRequest
{
    [Required]
    public string UserId { get; set; } = string.Empty;
    
    [Required]
    public string Username { get; set; } = string.Empty;
    
    [Required]
    public ulong ChannelId { get; set; }
    
    [Required]
    [Range(1, 1440, ErrorMessage = "Duration must be between 1 and 1440 minutes (24 hours)")]
    public int DurationMinutes { get; set; }
    
    public string? Message { get; set; }
}

public class TimerResponse
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public ulong ChannelId { get; set; }
    public int DurationMinutes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
    public bool IsCompleted { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string? Message { get; set; }
}

public class ExpiredTimersResponse
{
    public List<TimerResponse> ExpiredTimers { get; set; } = new();
}
