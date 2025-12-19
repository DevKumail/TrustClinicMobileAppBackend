using System.Data;
using CoherentMobile.Domain.Entities;
using CoherentMobile.Domain.Interfaces;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace CoherentMobile.Infrastructure.Repositories;

public class NotificationRepository : INotificationRepository
{
    private readonly string _connectionString;

    public NotificationRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new ArgumentNullException(nameof(configuration));
    }

    private IDbConnection CreateConnection() => new SqlConnection(_connectionString);

    public async Task<int> CreateAsync(AppNotification notification)
    {
        using var connection = CreateConnection();
        var sql = @"
INSERT INTO dbo.MNotifications (UserId, UserType, NotificationType, Title, Body, DataJson, CreatedAt, DeliveredAt, ReadAt, IsDeleted)
VALUES (@UserId, @UserType, @NotificationType, @Title, @Body, @DataJson, ISNULL(@CreatedAt, GETUTCDATE()), @DeliveredAt, @ReadAt, 0);
SELECT CAST(SCOPE_IDENTITY() as int);";

        var id = await connection.ExecuteScalarAsync<int>(sql, new
        {
            notification.UserId,
            notification.UserType,
            notification.NotificationType,
            notification.Title,
            notification.Body,
            notification.DataJson,
            notification.CreatedAt,
            notification.DeliveredAt,
            notification.ReadAt
        });

        return id;
    }

    public async Task<IReadOnlyList<AppNotification>> GetSinceAsync(int userId, string userType, DateTime? sinceUtc, int limit)
    {
        using var connection = CreateConnection();

        var effectiveLimit = limit <= 0 ? 100 : Math.Min(limit, 500);

        var sql = @"
SELECT TOP (@Limit)
    NotificationId,
    UserId,
    UserType,
    NotificationType,
    Title,
    Body,
    DataJson,
    CreatedAt,
    DeliveredAt,
    ReadAt,
    IsDeleted
FROM dbo.MNotifications
WHERE UserId = @UserId
  AND UserType = @UserType
  AND IsDeleted = 0
  AND (@SinceUtc IS NULL OR CreatedAt > @SinceUtc)
ORDER BY CreatedAt ASC;";

        var rows = await connection.QueryAsync<AppNotification>(sql, new
        {
            UserId = userId,
            UserType = userType,
            SinceUtc = sinceUtc,
            Limit = effectiveLimit
        });

        return rows.ToList();
    }

    public async Task<int> AckAsync(int userId, string userType, IReadOnlyList<int> notificationIds, bool markRead)
    {
        if (notificationIds.Count == 0)
            return 0;

        using var connection = CreateConnection();

        var sql = markRead
            ? @"
UPDATE dbo.MNotifications
SET
    DeliveredAt = COALESCE(DeliveredAt, GETUTCDATE()),
    ReadAt = COALESCE(ReadAt, GETUTCDATE())
WHERE UserId = @UserId
  AND UserType = @UserType
  AND NotificationId IN @Ids
  AND IsDeleted = 0;"
            : @"
UPDATE dbo.MNotifications
SET
    DeliveredAt = COALESCE(DeliveredAt, GETUTCDATE())
WHERE UserId = @UserId
  AND UserType = @UserType
  AND NotificationId IN @Ids
  AND IsDeleted = 0;";

        var affected = await connection.ExecuteAsync(sql, new { UserId = userId, UserType = userType, Ids = notificationIds.ToArray() });
        return affected;
    }
}
