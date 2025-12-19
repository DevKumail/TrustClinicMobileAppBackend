using System.Data;
using CoherentMobile.Domain.Entities;
using CoherentMobile.Domain.Interfaces;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace CoherentMobile.Infrastructure.Repositories;

public class DeviceTokenRepository : IDeviceTokenRepository
{
    private readonly string _connectionString;

    public DeviceTokenRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new ArgumentNullException(nameof(configuration));
    }

    private IDbConnection CreateConnection() => new SqlConnection(_connectionString);

    public async Task UpsertAsync(int userId, string userType, string token, string? platform)
    {
        using var connection = CreateConnection();

        var sql = @"
MERGE dbo.MDeviceTokens AS target
USING (SELECT @UserId AS UserId, @UserType AS UserType, @Token AS Token) AS source
ON (target.UserId = source.UserId AND target.UserType = source.UserType AND target.Token = source.Token)
WHEN MATCHED THEN
    UPDATE SET
        Platform = @Platform,
        IsActive = 1,
        UpdatedAt = GETUTCDATE()
WHEN NOT MATCHED THEN
    INSERT (UserId, UserType, Token, Platform, IsActive, CreatedAt)
    VALUES (@UserId, @UserType, @Token, @Platform, 1, GETUTCDATE());";

        await connection.ExecuteAsync(sql, new { UserId = userId, UserType = userType, Token = token, Platform = platform });
    }

    public async Task<IReadOnlyList<DeviceToken>> GetActiveAsync(int userId, string userType)
    {
        using var connection = CreateConnection();

        var sql = @"
SELECT
    DeviceTokenId,
    UserId,
    UserType,
    Token,
    Platform,
    IsActive,
    CreatedAt,
    UpdatedAt
FROM dbo.MDeviceTokens
WHERE UserId = @UserId
  AND UserType = @UserType
  AND IsActive = 1;";

        var rows = await connection.QueryAsync<DeviceToken>(sql, new { UserId = userId, UserType = userType });
        return rows.ToList();
    }

    public async Task DeactivateAsync(int userId, string userType, string token)
    {
        using var connection = CreateConnection();

        var sql = @"
UPDATE dbo.MDeviceTokens
SET IsActive = 0,
    UpdatedAt = GETUTCDATE()
WHERE UserId = @UserId
  AND UserType = @UserType
  AND Token = @Token;";

        await connection.ExecuteAsync(sql, new { UserId = userId, UserType = userType, Token = token });
    }
}
