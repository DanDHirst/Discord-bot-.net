using API.Data;
using API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TimerController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<TimerController> _logger;

    public TimerController(ApplicationDbContext context, ILogger<TimerController> logger)
    {
        _context = context;
        _logger = logger;
    }

    [HttpPost]
    public async Task<ActionResult<TimerResponse>> CreateTimer(CreateTimerRequest request)
    {
        _logger.LogInformation($"Creating timer for user {request.Username} ({request.UserId}) - {request.DurationMinutes} minutes");

        var now = DateTime.UtcNow;
        var timer = new API.Models.Timer
        {
            UserId = request.UserId,
            Username = request.Username,
            ChannelId = request.ChannelId,
            DurationMinutes = request.DurationMinutes,
            CreatedAt = now,
            ExpiresAt = now.AddMinutes(request.DurationMinutes),
            IsCompleted = false,
            Message = request.Message
        };

        _context.Timers.Add(timer);
        await _context.SaveChangesAsync();

        _logger.LogInformation($"Timer created with ID {timer.Id}, expires at {timer.ExpiresAt:yyyy-MM-dd HH:mm:ss} UTC");

        var response = new TimerResponse
        {
            Id = timer.Id,
            UserId = timer.UserId,
            Username = timer.Username,
            ChannelId = timer.ChannelId,
            DurationMinutes = timer.DurationMinutes,
            CreatedAt = timer.CreatedAt,
            ExpiresAt = timer.ExpiresAt,
            IsCompleted = timer.IsCompleted,
            CompletedAt = timer.CompletedAt,
            Message = timer.Message
        };

        return CreatedAtAction(nameof(GetTimer), new { id = timer.Id }, response);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<TimerResponse>> GetTimer(int id)
    {
        var timer = await _context.Timers.FindAsync(id);
        
        if (timer == null)
        {
            return NotFound();
        }

        var response = new TimerResponse
        {
            Id = timer.Id,
            UserId = timer.UserId,
            Username = timer.Username,
            ChannelId = timer.ChannelId,
            DurationMinutes = timer.DurationMinutes,
            CreatedAt = timer.CreatedAt,
            ExpiresAt = timer.ExpiresAt,
            IsCompleted = timer.IsCompleted,
            CompletedAt = timer.CompletedAt,
            Message = timer.Message
        };

        return response;
    }

    [HttpGet("expired")]
    public async Task<ActionResult<ExpiredTimersResponse>> GetExpiredTimers()
    {
        var now = DateTime.UtcNow;
        var expiredTimers = await _context.Timers
            .Where(t => !t.IsCompleted && t.ExpiresAt <= now)
            .ToListAsync();

        _logger.LogInformation($"Found {expiredTimers.Count} expired timers");

        var response = new ExpiredTimersResponse
        {
            ExpiredTimers = expiredTimers.Select(t => new TimerResponse
            {
                Id = t.Id,
                UserId = t.UserId,
                Username = t.Username,
                ChannelId = t.ChannelId,
                DurationMinutes = t.DurationMinutes,
                CreatedAt = t.CreatedAt,
                ExpiresAt = t.ExpiresAt,
                IsCompleted = t.IsCompleted,
                CompletedAt = t.CompletedAt,
                Message = t.Message
            }).ToList()
        };

        return response;
    }

    [HttpPost("{id}/complete")]
    public async Task<ActionResult> CompleteTimer(int id)
    {
        var timer = await _context.Timers.FindAsync(id);
        
        if (timer == null)
        {
            return NotFound();
        }

        if (timer.IsCompleted)
        {
            return BadRequest("Timer is already completed");
        }

        timer.IsCompleted = true;
        timer.CompletedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        _logger.LogInformation($"Timer {id} marked as completed");

        return NoContent();
    }

    [HttpGet("user/{userId}")]
    public async Task<ActionResult<List<TimerResponse>>> GetUserTimers(string userId, [FromQuery] bool includeCompleted = false)
    {
        var query = _context.Timers.Where(t => t.UserId == userId);
        
        if (!includeCompleted)
        {
            query = query.Where(t => !t.IsCompleted);
        }

        var timers = await query.OrderByDescending(t => t.CreatedAt).ToListAsync();

        var response = timers.Select(t => new TimerResponse
        {
            Id = t.Id,
            UserId = t.UserId,
            Username = t.Username,
            ChannelId = t.ChannelId,
            DurationMinutes = t.DurationMinutes,
            CreatedAt = t.CreatedAt,
            ExpiresAt = t.ExpiresAt,
            IsCompleted = t.IsCompleted,
            CompletedAt = t.CompletedAt,
            Message = t.Message
        }).ToList();

        return response;
    }
}
