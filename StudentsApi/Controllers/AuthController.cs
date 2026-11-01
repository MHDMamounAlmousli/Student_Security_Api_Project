using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using StudentBusiness;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using StudentApi.DTOs.Auth;
using Microsoft.AspNetCore.RateLimiting;
namespace StudentsApi;


[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly ILogger<AuthController> _logger;
    private readonly JWTService _jwtService;

    public AuthController(ILogger<AuthController> logger,JWTService jwtService)
    {
        _logger = logger;
        _jwtService = jwtService;
    }

    [EnableRateLimiting("AuthLimiter")]
    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh([FromBody] RefreshRequest request)
    {
        
        var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var refreshToken = await Task.Run(() => RefreshToken.Find(request.RefreshToken));
        bool isValid = BCrypt.Net.BCrypt.Verify(request.RefreshToken, refreshToken!.TokenHash);
        if (refreshToken is null || refreshToken.IsRevoked || refreshToken.ExpiresAt <= DateTime.UtcNow)
        {
            _logger.LogWarning("Invalid refresh attempt (token expired). Token={Token}, IP={IP}", request.RefreshToken, ip);
            return Unauthorized("Refresh token expired");
        }
        if(refreshToken.IsRevoked)
        {
            _logger.LogWarning("Invalid refresh attempt (token revoked). Token={Token}, IP={IP}", request.RefreshToken, ip);
            return Unauthorized("Refresh token revoked");
        }
        if(!isValid)
        {
            _logger.LogWarning("Invalid refresh token attempt. Token={Token}, IP={IP}", request.RefreshToken, ip);
            return Unauthorized("Invalid refresh token");
        }

        var student = await Task.Run(() => Student.Find(refreshToken.StudentId));
        if (student is null || !string.Equals(student.Email, request.Email, StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogWarning("Invalid refresh attempt (email not found). Email={Email}, IP={IP}", request.Email, ip);
            return Unauthorized("Invalid credentials");
        }
        
        var newAccessToken = _jwtService.GenerateJwtToken(student);

        var newRefreshToken = RefreshToken.GenerateRefreshToken();
        await Task.Run(() => RefreshToken.Revoke(request.RefreshToken));
        await Task.Run(() => new RefreshToken
        {
            StudentId = student.ID,
            TokenHash = newRefreshToken,
            ExpiresAt = DateTime.UtcNow.AddDays(7)
        }.Save());

        _logger.LogInformation(
               "Refresh succeeded. UserId={UserId}, Email={Email}, IP={IP}",
               student.ID,
               student.Email,
               ip
           );//notice : not neccessary to log the Refresh succeeded

        return Ok(new TokenResponse
        {
            AccessToken = newAccessToken,
            RefreshToken = newRefreshToken
        });
    }

    [EnableRateLimiting("AuthLimiter")]
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginReq request)
    {
        var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var student = await Task.Run(() => Student.FindByEmail(request.Email));
        if (student is null)
        {
            _logger.LogWarning("Failed login attempt (email not found). Email={Email}, IP={IP}", request.Email, ip);
            return Unauthorized("Invalid Credetails");
        }
        bool isValid = BCrypt.Net.BCrypt.Verify(request.Password, student.Password);
        if (!isValid)
        {
            _logger.LogWarning("Failed login attempt (invalid credentials). Email={Email}, IP={IP}", request.Email, ip);
            return Unauthorized("Invalid Credetails");
        }
        var accessToken = _jwtService.GenerateJwtToken(student);

        var refreshToken = RefreshToken.GenerateRefreshToken();

        await Task.Run(() => new RefreshToken
        {
            StudentId = student.ID,
            TokenHash = refreshToken,
            ExpiresAt = DateTime.UtcNow.AddDays(7)
        }.Save());
        _logger.LogInformation("Refresh succeeded. UserId={UserId}, Email={Email}, IP={IP}", student.ID, student.Email, ip);//notice : not neccessary to login succeeded
        return Ok(new TokenResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken
        });
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout([FromBody] LogoutRequest request)
    {
        var refreshToken = await Task.Run(() => RefreshToken.Find(request.RefreshToken));
        if (refreshToken is null || refreshToken.IsRevoked)
            return Ok();

        var student = await Task.Run(() => Student.Find(refreshToken.StudentId));
        if (student is null || !string.Equals(student.Email, request.Email, StringComparison.OrdinalIgnoreCase))
            return Ok();

        await Task.Run(() => RefreshToken.Revoke(request.RefreshToken));
        return Ok("Logged out successfully");
    }
}