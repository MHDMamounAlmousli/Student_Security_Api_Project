using System.Security.Cryptography;
using System.Text;
using StudentDataAccess;

namespace StudentBusiness;

public interface IRefreshToken
{
    RefreshTokenDTO RTDTO { get; }
    int Id { get; set; }
    int StudentId { get; set; }
    string TokenHash { get; set; }
    DateTime ExpiresAt { get; set; }
    bool IsRevoked { get; set; }
    DateTime CreatedAt { get; set; }

    static abstract RefreshToken? Find(string token);
    static abstract string GenerateRefreshToken();
    static abstract void Revoke(string token);
    void Save();
}

public interface IRefreshToken1
{
    RefreshTokenDTO RTDTO { get; }
    int Id { get; set; }
    int StudentId { get; set; }
    string TokenHash { get; set; }
    DateTime ExpiresAt { get; set; }
    bool IsRevoked { get; set; }
    DateTime CreatedAt { get; set; }

    static abstract RefreshToken? Find(string token);
    static abstract string GenerateRefreshToken();
    static abstract void Revoke(string token);
    void Save();
}

public class RefreshToken 
{
    public RefreshTokenDTO RTDTO
    {
        get
        {
            return new RefreshTokenDTO
            {
                Id = this.Id,
                StudentId = this.StudentId,
                TokenHash = this.TokenHash,
                ExpiresAt = this.ExpiresAt,
                IsRevoked = this.IsRevoked,
                CreatedAt = this.CreatedAt
            };
        }
    }

    public int Id { get; set; }
    public int StudentId { get; set; }
    public string TokenHash { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public bool IsRevoked { get; set; }
    public DateTime CreatedAt { get; set; }

    public RefreshToken()
    {
    }

    public void Save()
    {
        RefreshTokenDataAccess.SaveRefreshToken(this.StudentId, HashToken(this.TokenHash), this.ExpiresAt);
    }

    public static RefreshToken? Find(string token)
    {
        RefreshTokenDTO? refreshToken = RefreshTokenDataAccess.GetByTokenHash(HashToken(token));

        if (refreshToken != null)
        {
            return new RefreshToken(refreshToken);
        }

        return null;
    }

    public static void Revoke(string token)
    {
        RefreshTokenDataAccess.RevokeRefreshToken(HashToken(token));
    }

    private static string HashToken(string token)
    {
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(token));
        return Convert.ToHexString(hash);
    }

    public static string GenerateRefreshToken()
    {
        var bytes = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(bytes);
        return Convert.ToBase64String(bytes);
    }
    private RefreshToken(RefreshTokenDTO refreshToken)
    {
        Id = refreshToken.Id;
        StudentId = refreshToken.StudentId;
        TokenHash = refreshToken.TokenHash;
        ExpiresAt = refreshToken.ExpiresAt;
        IsRevoked = refreshToken.IsRevoked;
        CreatedAt = refreshToken.CreatedAt;
    }
}