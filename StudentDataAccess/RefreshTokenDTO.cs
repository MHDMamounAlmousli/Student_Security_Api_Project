namespace StudentDataAccess;

public class RefreshTokenDTO
{
    public int Id { get; set; }
    public int StudentId { get; set; }
    public string TokenHash { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public bool IsRevoked { get; set; }
    public DateTime CreatedAt { get; set; }
}