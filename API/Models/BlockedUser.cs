using System.ComponentModel.DataAnnotations;

namespace API.Models;

public class BlockedUser
{
    public int Id { get; set; }
    
    [Required]
    public string UserId { get; set; } = string.Empty;
    
    [Required]
    public string Username { get; set; } = string.Empty;
    
    public DateTime BlockedAt { get; set; }
    
    public string? Reason { get; set; }
    
    [Required]
    public string BlockedBy { get; set; } = string.Empty; // Who blocked this user
}
