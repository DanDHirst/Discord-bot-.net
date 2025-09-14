using API.Data;
using API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BlockedUsersController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<BlockedUsersController> _logger;

    public BlockedUsersController(ApplicationDbContext context, ILogger<BlockedUsersController> logger)
    {
        _context = context;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<BlockedUsersResponse>> GetBlockedUsers()
    {
        _logger.LogInformation("Retrieving all blocked users");

        var blockedUsers = await _context.BlockedUsers
            .OrderByDescending(b => b.BlockedAt)
            .ToListAsync();

        var response = new BlockedUsersResponse
        {
            BlockedUsers = blockedUsers.Select(b => new BlockedUserResponse
            {
                Id = b.Id,
                UserId = b.UserId,
                Username = b.Username,
                BlockedAt = b.BlockedAt,
                Reason = b.Reason,
                BlockedBy = b.BlockedBy
            }).ToList()
        };

        return response;
    }

    [HttpPost]
    public async Task<ActionResult<BlockedUserResponse>> BlockUser(CreateBlockedUserRequest request)
    {
        _logger.LogInformation($"Blocking user {request.Username} ({request.UserId})");

        // Check if user is already blocked
        var existingBlock = await _context.BlockedUsers
            .FirstOrDefaultAsync(b => b.UserId == request.UserId);

        if (existingBlock != null)
        {
            return Conflict(new { message = $"User {request.Username} is already blocked" });
        }

        var blockedUser = new BlockedUser
        {
            UserId = request.UserId,
            Username = request.Username,
            BlockedAt = DateTime.UtcNow,
            Reason = request.Reason,
            BlockedBy = request.BlockedBy
        };

        _context.BlockedUsers.Add(blockedUser);
        await _context.SaveChangesAsync();

        _logger.LogInformation($"User {request.Username} blocked successfully with ID {blockedUser.Id}");

        var response = new BlockedUserResponse
        {
            Id = blockedUser.Id,
            UserId = blockedUser.UserId,
            Username = blockedUser.Username,
            BlockedAt = blockedUser.BlockedAt,
            Reason = blockedUser.Reason,
            BlockedBy = blockedUser.BlockedBy
        };

        return CreatedAtAction(nameof(GetBlockedUser), new { id = blockedUser.Id }, response);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<BlockedUserResponse>> GetBlockedUser(int id)
    {
        var blockedUser = await _context.BlockedUsers.FindAsync(id);

        if (blockedUser == null)
        {
            return NotFound();
        }

        var response = new BlockedUserResponse
        {
            Id = blockedUser.Id,
            UserId = blockedUser.UserId,
            Username = blockedUser.Username,
            BlockedAt = blockedUser.BlockedAt,
            Reason = blockedUser.Reason,
            BlockedBy = blockedUser.BlockedBy
        };

        return response;
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> UnblockUser(int id)
    {
        var blockedUser = await _context.BlockedUsers.FindAsync(id);

        if (blockedUser == null)
        {
            return NotFound();
        }

        _context.BlockedUsers.Remove(blockedUser);
        await _context.SaveChangesAsync();

        _logger.LogInformation($"User {blockedUser.Username} ({blockedUser.UserId}) unblocked successfully");

        return NoContent();
    }

    [HttpGet("check/{userId}")]
    public async Task<ActionResult<bool>> IsUserBlocked(string userId)
    {
        var isBlocked = await _context.BlockedUsers
            .AnyAsync(b => b.UserId == userId);

        return isBlocked;
    }
}
