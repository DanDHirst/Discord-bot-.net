using System.ComponentModel.DataAnnotations;

namespace API.Models;

public class Timer
{
    public int Id { get; set; }
    
    [Required]
    public string UserId { get; set; } = string.Empty;
    
    [Required]
    public string Username { get; set; } = string.Empty;
    
    [Required]
    public ulong ChannelId { get; set; }
    
    [Required]
    public int DurationMinutes { get; set; }
    
    [Required]
    public DateTime CreatedAt { get; set; }
    
    [Required]
    public DateTime ExpiresAt { get; set; }
    
    public bool IsCompleted { get; set; }
    
    public DateTime? CompletedAt { get; set; }
    
    public string? Message { get; set; }
}
