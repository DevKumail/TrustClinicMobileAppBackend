
using System.Data;
using System;
using CoherentMobile.Domain.Entities;
using CoherentMobile.Domain.Interfaces;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace CoherentMobile.Infrastructure.Repositories;

public class MedicationReminderRepository : IMedicationReminderRepository
{
    private readonly string _connectionString;

    public MedicationReminderRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new ArgumentNullException(nameof(configuration));
    }

    private IDbConnection CreateConnection() => new SqlConnection(_connectionString);

    public async Task<int> CreateAsync(MedicationReminder reminder)
    {
        using var connection = CreateConnection();

        var sql = @"
INSERT INTO dbo.MMedicationReminders
    (UserId, UserType, MedicationName, Dosage, Title, Body, DataJson, NextTriggerAtUtc, RepeatIntervalMinutes, LastTriggeredAtUtc, IsActive, CreatedAtUtc, UpdatedAtUtc)
VALUES
    (@UserId, @UserType, @MedicationName, @Dosage, @Title, @Body, @DataJson, @NextTriggerAtUtc, @RepeatIntervalMinutes, @LastTriggeredAtUtc, @IsActive, ISNULL(@CreatedAtUtc, GETUTCDATE()), @UpdatedAtUtc);
SELECT CAST(SCOPE_IDENTITY() AS INT);";

        return await connection.ExecuteScalarAsync<int>(sql, new
        {
            reminder.UserId,
            reminder.UserType,
            reminder.MedicationName,
            reminder.Dosage,
            reminder.Title,
            reminder.Body,
            reminder.DataJson,
            reminder.NextTriggerAtUtc,
            reminder.RepeatIntervalMinutes,
            reminder.LastTriggeredAtUtc,
            reminder.IsActive,
            reminder.CreatedAtUtc,
            reminder.UpdatedAtUtc
        });
    }

    public async Task<int> UpdateAsync(MedicationReminder reminder)
    {
        using var connection = CreateConnection();

        var sql = @"
UPDATE dbo.MMedicationReminders
SET
    MedicationName = @MedicationName,
    Dosage = @Dosage,
    Title = @Title,
    Body = @Body,
    DataJson = @DataJson,
    NextTriggerAtUtc = @NextTriggerAtUtc,
    RepeatIntervalMinutes = @RepeatIntervalMinutes,
    UpdatedAtUtc = ISNULL(@UpdatedAtUtc, GETUTCDATE())
WHERE MedicationReminderId = @MedicationReminderId
  AND IsActive = 1;";

        return await connection.ExecuteAsync(sql, new
        {
            reminder.MedicationReminderId,
            reminder.MedicationName,
            reminder.Dosage,
            reminder.Title,
            reminder.Body,
            reminder.DataJson,
            reminder.NextTriggerAtUtc,
            reminder.RepeatIntervalMinutes,
            reminder.UpdatedAtUtc
        });
    }

    public async Task<MedicationReminder?> GetByIdAsync(int medicationReminderId)
    {
        using var connection = CreateConnection();
        var sql = @"SELECT * FROM dbo.MMedicationReminders WHERE MedicationReminderId = @MedicationReminderId";
        return await connection.QueryFirstOrDefaultAsync<MedicationReminder>(sql, new { MedicationReminderId = medicationReminderId });
    }

    public async Task<IReadOnlyList<MedicationReminder>> GetUserAsync(int userId, string userType, bool activeOnly = true)
    {
        using var connection = CreateConnection();

        var sql = activeOnly
            ? @"
SELECT *
FROM dbo.MMedicationReminders
WHERE UserId = @UserId
  AND UserType = @UserType
  AND IsActive = 1
ORDER BY NextTriggerAtUtc ASC, MedicationReminderId ASC;"
            : @"
SELECT *
FROM dbo.MMedicationReminders
WHERE UserId = @UserId
  AND UserType = @UserType
ORDER BY IsActive DESC, NextTriggerAtUtc ASC, MedicationReminderId ASC;";

        var rows = await connection.QueryAsync<MedicationReminder>(sql, new { UserId = userId, UserType = userType });
        return rows.ToList();
    }

    public async Task<IReadOnlyList<MedicationReminder>> GetDueAsync(DateTime nowUtc, int take = 200)
    {
        using var connection = CreateConnection();

        var effectiveTake = take <= 0 ? 200 : Math.Min(take, 500);

        var sql = @"
SELECT TOP (@Take)
    *
FROM dbo.MMedicationReminders
WHERE IsActive = 1
  AND NextTriggerAtUtc <= @NowUtc
ORDER BY NextTriggerAtUtc ASC, MedicationReminderId ASC;";

        var rows = await connection.QueryAsync<MedicationReminder>(sql, new { NowUtc = nowUtc, Take = effectiveTake });
        return rows.ToList();
    }

    public async Task<int> MarkTriggeredAsync(int medicationReminderId, DateTime lastTriggeredAtUtc, DateTime? nextTriggerAtUtc, bool isActive)
    {
        using var connection = CreateConnection();

        var sql = @"
UPDATE dbo.MMedicationReminders
SET
    LastTriggeredAtUtc = @LastTriggeredAtUtc,
    NextTriggerAtUtc = COALESCE(@NextTriggerAtUtc, NextTriggerAtUtc),
    IsActive = @IsActive,
    UpdatedAtUtc = GETUTCDATE()
WHERE MedicationReminderId = @MedicationReminderId
  AND IsActive = 1;";

        return await connection.ExecuteAsync(sql, new
        {
            MedicationReminderId = medicationReminderId,
            LastTriggeredAtUtc = lastTriggeredAtUtc,
            NextTriggerAtUtc = nextTriggerAtUtc,
            IsActive = isActive ? 1 : 0
        });
    }

    public async Task<int> DeactivateAsync(int medicationReminderId, int userId, string userType)
    {
        using var connection = CreateConnection();

        var sql = @"
UPDATE dbo.MMedicationReminders
SET IsActive = 0,
    UpdatedAtUtc = GETUTCDATE()
WHERE MedicationReminderId = @MedicationReminderId
  AND UserId = @UserId
  AND UserType = @UserType
  AND IsActive = 1;";

        return await connection.ExecuteAsync(sql, new { MedicationReminderId = medicationReminderId, UserId = userId, UserType = userType });
    }

    public async Task<int> ApplyActionAsync(int medicationReminderId, int userId, string userType, string action, DateTime nowUtc)
    {
        using var connection = CreateConnection();

        var raw = (action ?? string.Empty).Trim();

        int? actionTypeId = null;
        if (int.TryParse(raw, out var parsedId) && parsedId > 0)
        {
            actionTypeId = parsedId;
        }

        var normalized = raw.ToLowerInvariant();
        if (actionTypeId == null)
        {
            actionTypeId = normalized switch
            {
                "taken" => 1,
                "not taken" => 2,
                "not_taken" => 2,
                "nottaken" => 2,
                "remind me later" => 3,
                "remind_me_later" => 3,
                "remindmelater" => 3,
                "later" => 3,
                _ => null
            };
        }

        if (actionTypeId is 1 or 2)
        {
            var sql = @"
UPDATE dbo.MMedicationReminders
SET IsActive = 0,
    LastActionTypeId = @LastActionTypeId,
    LastActionAtUtc = @NowUtc,
    UpdatedAtUtc = GETUTCDATE()
WHERE MedicationReminderId = @MedicationReminderId
  AND UserId = @UserId
  AND UserType = @UserType
  AND IsActive = 1;";

            return await connection.ExecuteAsync(sql, new
            {
                MedicationReminderId = medicationReminderId,
                UserId = userId,
                UserType = userType,
                LastActionTypeId = actionTypeId,
                NowUtc = nowUtc
            });
        }

        if (actionTypeId == 3)
        {
            var next = nowUtc.AddMinutes(5);

            var sql = @"
UPDATE dbo.MMedicationReminders
SET NextTriggerAtUtc = @NextTriggerAtUtc,
    LastActionTypeId = @LastActionTypeId,
    LastActionAtUtc = @NowUtc,
    UpdatedAtUtc = GETUTCDATE()
WHERE MedicationReminderId = @MedicationReminderId
  AND UserId = @UserId
  AND UserType = @UserType
  AND IsActive = 1;";

            return await connection.ExecuteAsync(sql, new
            {
                MedicationReminderId = medicationReminderId,
                UserId = userId,
                UserType = userType,
                NextTriggerAtUtc = next,
                LastActionTypeId = actionTypeId,
                NowUtc = nowUtc
            });
        }

        throw new ArgumentException("Invalid action. Allowed: Taken(1), NotTaken(2), RemindMeLater(3)", nameof(action));
    }
}

