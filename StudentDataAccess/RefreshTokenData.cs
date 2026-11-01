using System.Data;
using Microsoft.Data.SqlClient;

namespace StudentDataAccess;

public class RefreshTokenDataAccess
{
    private static readonly string _connectionString = DatabaseConnection.ConnectionString;

    public RefreshTokenDataAccess()
    {
        
    }

    public static void SaveRefreshToken(int studentId, string tokenHash, DateTime expiresAt)
    {
        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand("SP_SaveRefreshToken", connection)
        {
            CommandType = CommandType.StoredProcedure
        };

        command.Parameters.AddWithValue("@StudentId", studentId);
        command.Parameters.AddWithValue("@TokenHash", tokenHash);
        command.Parameters.AddWithValue("@ExpiresAt", expiresAt);

        connection.Open();
        command.ExecuteNonQuery();
    }

    public static RefreshTokenDTO? GetByTokenHash(string tokenHash)
    {
        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand("SP_GetRefreshTokenByHash", connection)
        {
            CommandType = CommandType.StoredProcedure
        };

        command.Parameters.AddWithValue("@TokenHash", tokenHash);

        connection.Open();
        using var reader = command.ExecuteReader();
        if (reader.Read())
        {
            return new RefreshTokenDTO
            {
                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                StudentId = reader.GetInt32(reader.GetOrdinal("StudentId")),
                TokenHash = reader.GetString(reader.GetOrdinal("TokenHash")),
                ExpiresAt = reader.GetDateTime(reader.GetOrdinal("ExpiresAt")),
                IsRevoked = reader.GetBoolean(reader.GetOrdinal("IsRevoked")),
                CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt"))
            };
        }
        return null;
    }

    public static void RevokeRefreshToken(string tokenHash)
    {
        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand("SP_RevokeRefreshToken", connection)
        {
            CommandType = CommandType.StoredProcedure
        };

        command.Parameters.AddWithValue("@TokenHash", tokenHash);

        connection.Open();
        command.ExecuteNonQuery();
    }
}