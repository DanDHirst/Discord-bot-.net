using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IConfiguration configuration, ILogger<AuthController> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    [HttpPost("token")]
    public ActionResult<TokenResponse> GetToken([FromBody] TokenRequest request)
    {
        // Verify the API key
        var configApiKey = _configuration["ApiKey"];
        if (string.IsNullOrEmpty(configApiKey) || request.ApiKey != configApiKey)
        {
            _logger.LogWarning("Invalid API key provided");
            return Unauthorized(new { message = "Invalid API key" });
        }

        // Generate JWT token
        var token = GenerateJwtToken();
        var expiryMinutes = _configuration.GetValue<int>("Jwt:ExpiryMinutes", 1440);
        var expiresAt = DateTime.UtcNow.AddMinutes(expiryMinutes);

        _logger.LogInformation("JWT token generated successfully");

        return Ok(new TokenResponse 
        { 
            Token = token, 
            ExpiresAt = expiresAt,
            TokenType = "Bearer"
        });
    }

    private string GenerateJwtToken()
    {
        var secretKey = _configuration["Jwt:SecretKey"];
        var issuer = _configuration["Jwt:Issuer"];
        var audience = _configuration["Jwt:Audience"];
        var expiryMinutes = _configuration.GetValue<int>("Jwt:ExpiryMinutes", 1440);

        var key = Encoding.ASCII.GetBytes(secretKey!);
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim("sub", "discord-bot"),
                new Claim("iss", issuer!),
                new Claim("aud", audience!),
                new Claim("iat", DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
            }),
            Expires = DateTime.UtcNow.AddMinutes(expiryMinutes),
            Issuer = issuer,
            Audience = audience,
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
}

public class TokenRequest
{
    public string ApiKey { get; set; } = string.Empty;
}

public class TokenResponse
{
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public string TokenType { get; set; } = string.Empty;
}
