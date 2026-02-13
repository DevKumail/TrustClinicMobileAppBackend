
using System.Text.Json;
using CoherentMobile.Application.DTOs;
using CoherentMobile.Application.Interfaces;
using CoherentMobile.Domain.Entities;
using CoherentMobile.Domain.Interfaces;

namespace CoherentMobile.Application.Services;

public class MedicationReminderService : IMedicationReminderService
{
    private readonly IMedicationReminderRepository _repo;
    private readonly INotificationService _notifications;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    public MedicationReminderService(IMedicationReminderRepository repo, INotificationService notifications)
    {
        _repo = repo;
        _notifications = notifications;
    }

    public async Task<IReadOnlyList<MedicationReminderDto>> GetMyAsync(int userId, string userType, bool activeOnly = true)
    {
        var items = await _repo.GetUserAsync(userId, userType, activeOnly);
        return items.Select(x => new MedicationReminderDto
        {
            MedicationReminderId = x.MedicationReminderId,
            MedicationName = x.MedicationName,
            Dosage = x.Dosage,
            Title = x.Title,
            Body = x.Body,
            DataJson = x.DataJson,
            NextTriggerAtUtc = x.NextTriggerAtUtc,
            RepeatIntervalMinutes = x.RepeatIntervalMinutes,
            LastTriggeredAtUtc = x.LastTriggeredAtUtc,
            IsActive = x.IsActive
        }).ToList();
    }

    public async Task<int> CreateAsync(int userId, string userType, MedicationReminderUpsertRequestDto request)
    {
        if (request.NextTriggerAtUtc == default)
            throw new ArgumentException("NextTriggerAtUtc is required", nameof(request));

        var reminder = new MedicationReminder
        {
            UserId = userId,
            UserType = userType,
            MedicationName = request.MedicationName,
            Dosage = request.Dosage,
            Title = request.Title,
            Body = request.Body,
            DataJson = request.DataJson,
            NextTriggerAtUtc = request.NextTriggerAtUtc.ToUniversalTime(),
            RepeatIntervalMinutes = request.RepeatIntervalMinutes,
            LastTriggeredAtUtc = null,
            IsActive = true,
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = null
        };

        return await _repo.CreateAsync(reminder);
    }

    public async Task<bool> UpdateAsync(int userId, string userType, int medicationReminderId, MedicationReminderUpsertRequestDto request)
    {
        var existing = await _repo.GetByIdAsync(medicationReminderId);
        if (existing == null)
            return false;

        if (existing.UserId != userId || !existing.UserType.Equals(userType, StringComparison.OrdinalIgnoreCase))
            return false;

        if (request.NextTriggerAtUtc == default)
            throw new ArgumentException("NextTriggerAtUtc is required", nameof(request));

        existing.MedicationName = request.MedicationName;
        existing.Dosage = request.Dosage;
        existing.Title = request.Title;
        existing.Body = request.Body;
        existing.DataJson = request.DataJson;
        existing.NextTriggerAtUtc = request.NextTriggerAtUtc.ToUniversalTime();
        existing.RepeatIntervalMinutes = request.RepeatIntervalMinutes;
        existing.UpdatedAtUtc = DateTime.UtcNow;

        var affected = await _repo.UpdateAsync(existing);
        return affected > 0;
    }

    public async Task<bool> DeactivateAsync(int userId, string userType, int medicationReminderId)
    {
        var affected = await _repo.DeactivateAsync(medicationReminderId, userId, userType);
        return affected > 0;
    }

    public async Task<bool> ApplyActionAsync(int userId, string userType, int medicationReminderId, MedicationReminderActionRequestDto request)
    {
        if (request == null)
            throw new ArgumentNullException(nameof(request));

        if (request.ActionId == null && string.IsNullOrWhiteSpace(request.Action))
            throw new ArgumentException("ActionId or Action is required", nameof(request));

        var existing = await _repo.GetByIdAsync(medicationReminderId);
        if (existing == null)
            return false;

        if (existing.UserId != userId || !existing.UserType.Equals(userType, StringComparison.OrdinalIgnoreCase))
            return false;

        var actionValue = request.ActionId.HasValue
            ? request.ActionId.Value.ToString()
            : request.Action;

        var affected = await _repo.ApplyActionAsync(medicationReminderId, userId, userType, actionValue, DateTime.UtcNow);
        return affected > 0;
    }

    public async Task<int> ProcessDueRemindersAsync(DateTime nowUtc, int take = 200)
    {
        var due = await _repo.GetDueAsync(nowUtc, take);
        var processed = 0;

        foreach (var r in due)
        {
            var title = string.IsNullOrWhiteSpace(r.Title) ? "Medication reminder" : r.Title;
            var body = string.IsNullOrWhiteSpace(r.Body) ? "Open app to view" : r.Body;

            var payload = new Dictionary<string, object?>
            {
                ["medicationReminderId"] = r.MedicationReminderId,
                ["medicationName"] = r.MedicationName,
                ["dosage"] = r.Dosage,
                ["triggeredAtUtc"] = nowUtc
            };

            var mergedJson = string.IsNullOrWhiteSpace(r.DataJson)
                ? JsonSerializer.Serialize(payload, JsonOptions)
                : r.DataJson;

            await _notifications.CreateAsync(new NotificationCreateRequestDto
            {
                UserId = r.UserId,
                UserType = r.UserType,
                NotificationType = "medication.reminder",
                Title = title,
                Body = body,
                DataJson = mergedJson
            });

            DateTime? nextTrigger = null;
            var stillActive = false;

            if (r.RepeatIntervalMinutes.HasValue && r.RepeatIntervalMinutes.Value > 0)
            {
                stillActive = true;
                nextTrigger = nowUtc.AddMinutes(r.RepeatIntervalMinutes.Value);
            }

            await _repo.MarkTriggeredAsync(r.MedicationReminderId, nowUtc, nextTrigger, stillActive);
            processed++;
        }

        return processed;
    }
}

