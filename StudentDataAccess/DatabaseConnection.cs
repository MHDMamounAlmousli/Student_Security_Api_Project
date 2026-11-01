using Microsoft.Extensions.Configuration;

namespace StudentDataAccess;

public static class DatabaseConnection
{
    private static string? _connectionString;

    public static string ConnectionString =>_connectionString ?? throw new InvalidOperationException("Database connection string is missing.");

    public static void Configure(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")?? throw new InvalidOperationException("ConnectionStrings:DefaultConnection is missing.");
    }
}