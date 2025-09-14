using API.Controllers;
using API.Data;
using API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace API.Tests;

/// <summary>
/// Simple unit tests demonstrating testing concepts for interview
/// These tests focus on core functionality without complex setup
/// </summary>
public class SimpleTests
{
    [Fact]
    public void AuthController_GetToken_InvalidApiKey_ReturnsUnauthorized()
    {
        // Arrange
        var mockConfig = new Mock<IConfiguration>();
        var mockLogger = new Mock<ILogger<AuthController>>();
        
        mockConfig.Setup(c => c["ApiKey"]).Returns("correct-key");
        
        var controller = new AuthController(mockConfig.Object, mockLogger.Object);
        var request = new TokenRequest { ApiKey = "wrong-key" };

        // Act
        var result = controller.GetToken(request);

        // Assert
        Assert.IsType<UnauthorizedObjectResult>(result.Result);
    }



    [Fact]
    public async Task BlockedUsersController_BlockUser_ValidRequest_ReturnsCreated()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        await using var context = new ApplicationDbContext(options);
        var mockLogger = new Mock<ILogger<BlockedUsersController>>();
        var controller = new BlockedUsersController(context, mockLogger.Object);

        var request = new CreateBlockedUserRequest
        {
            UserId = "123456789",
            Username = "testuser",
            Reason = "Testing",
            BlockedBy = "admin"
        };

        // Act
        var result = await controller.BlockUser(request);

        // Assert
        var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
        var response = Assert.IsType<BlockedUserResponse>(createdResult.Value);
        Assert.Equal(request.UserId, response.UserId);
        Assert.Equal(request.Username, response.Username);
    }

    [Fact]
    public async Task TimerController_CreateTimer_ValidRequest_ReturnsCreated()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        await using var context = new ApplicationDbContext(options);
        var mockLogger = new Mock<ILogger<TimerController>>();
        var controller = new TimerController(context, mockLogger.Object);

        var request = new CreateTimerRequest
        {
            UserId = "123456789",
            Username = "testuser",
            ChannelId = 987654321,
            DurationMinutes = 15,
            Message = "Test timer"
        };

        // Act
        var result = await controller.CreateTimer(request);

        // Assert
        var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
        var response = Assert.IsType<TimerResponse>(createdResult.Value);
        Assert.Equal(request.UserId, response.UserId);
        Assert.Equal(request.DurationMinutes, response.DurationMinutes);
        Assert.False(response.IsCompleted);
    }

    [Fact]
    public async Task TimerController_GetTimer_NonExistentId_ReturnsNotFound()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        await using var context = new ApplicationDbContext(options);
        var mockLogger = new Mock<ILogger<TimerController>>();
        var controller = new TimerController(context, mockLogger.Object);

        // Act
        var result = await controller.GetTimer(999);

        // Assert
        Assert.IsType<NotFoundResult>(result.Result);
    }

}
