using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace API.Models;

public class CreateBlockedUserRequest
{
    [Required]
    [JsonPropertyName("userId")]
    public string UserId { get; set; } = string.Empty;
    
    [Required]
    [JsonPropertyName("username")]
    public string Username { get; set; } = string.Empty;
    
    [JsonPropertyName("reason")]
    public string? Reason { get; set; }
    
    [Required]
    [JsonPropertyName("blockedBy")]
    public string BlockedBy { get; set; } = string.Empty;
}

public class BlockedUserResponse
{
    [JsonPropertyName("id")]
    public int Id { get; set; }
    
    [JsonPropertyName("userId")]
    public string UserId { get; set; } = string.Empty;
    
    [JsonPropertyName("username")]
    public string Username { get; set; } = string.Empty;
    
    [JsonPropertyName("blockedAt")]
    public DateTime BlockedAt { get; set; }
    
    [JsonPropertyName("reason")]
    public string? Reason { get; set; }
    
    [JsonPropertyName("blockedBy")]
    public string BlockedBy { get; set; } = string.Empty;
}

public class BlockedUsersResponse
{
    [JsonPropertyName("blockedUsers")]
    public List<BlockedUserResponse> BlockedUsers { get; set; } = new();
}
