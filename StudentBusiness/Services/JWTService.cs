using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace StudentBusiness;

public class JWTService
{
    private readonly IConfigurationSection jwtRefresh;
    public JWTService(IConfiguration configuration)
    {
        jwtRefresh = configuration.GetSection("Jwt");
    }
    public string GenerateJwtToken(Student student)
    {
        var claims = new[]
        {
        new Claim(ClaimTypes.NameIdentifier, student.ID.ToString()),
        new Claim(ClaimTypes.Email, student.Email),
        new Claim(ClaimTypes.Role, student.Role)
        };

        var secret = jwtRefresh["Key"] ?? throw new InvalidOperationException("Jwt Key is missing");
        var durationMinutes = double.Parse(jwtRefresh["DurationInMinutes"] ?? "30");
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: jwtRefresh["Issuer"] ?? "StudentApi",
            audience: jwtRefresh["Audience"] ?? "StudentApiUsers",
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(durationMinutes),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
