using CoherentMobile.Application.DTOs;
using CoherentMobile.Application.Interfaces;
using CoherentMobile.Domain.Entities;
using CoherentMobile.Domain.Interfaces;

namespace CoherentMobile.Application.Services;

public class NotificationService : INotificationService
{
    private readonly INotificationRepository _repository;
    private readonly IPushNotificationSender _push;

    public NotificationService(INotificationRepository repository, IPushNotificationSender push)
    {
        _repository = repository;
        _push = push;
    }

    public async Task<NotificationsGetResponseDto> GetSinceAsync(int userId, string userType, DateTime? sinceUtc, int limit)
    {
        var notifications = await _repository.GetSinceAsync(userId, userType, sinceUtc, limit);

        return new NotificationsGetResponseDto
        {
            ServerTimeUtc = DateTime.UtcNow,
            Notifications = notifications.Select(n => new NotificationDto
            {
                NotificationId = n.NotificationId,
                NotificationType = n.NotificationType,
                Title = n.Title,
                Body = n.Body,
                DataJson = n.DataJson,
                CreatedAt = n.CreatedAt,
                DeliveredAt = n.DeliveredAt,
                ReadAt = n.ReadAt
            }).ToList()
        };
    }

    public async Task<NotificationsAckResponseDto> AckAsync(int userId, string userType, NotificationsAckRequestDto request)
    {
        var ids = (request.NotificationIds ?? new List<int>()).Distinct().ToList();
        var affected = await _repository.AckAsync(userId, userType, ids, request.MarkRead);
        return new NotificationsAckResponseDto { AffectedCount = affected };
    }

    public async Task<int> CreateAsync(NotificationCreateRequestDto request)
    {
        var notification = new AppNotification
        {
            UserId = request.UserId,
            UserType = request.UserType,
            NotificationType = request.NotificationType,
            Title = request.Title,
            Body = request.Body,
            DataJson = request.DataJson,
            CreatedAt = DateTime.UtcNow,
            DeliveredAt = null,
            ReadAt = null,
            IsDeleted = false
        };

        var id = await _repository.CreateAsync(notification);

        var (safeTitle, safeBody) = GetSafePushContent(request.NotificationType);

        var pushData = new Dictionary<string, string>
        {
            ["notificationId"] = id.ToString(),
            ["notificationType"] = request.NotificationType
        };

        if (!string.IsNullOrWhiteSpace(request.DataJson))
        {
            pushData["dataJson"] = request.DataJson;
        }

        await _push.SendWakeupAsync(
            request.UserId,
            request.UserType,
            safeTitle,
            safeBody,
            pushData);

        return id;
    }

    private static (string title, string body) GetSafePushContent(string notificationType)
    {
        if (notificationType.StartsWith("chat", StringComparison.OrdinalIgnoreCase))
            return ("New message", "Open app to view");

        if (notificationType.StartsWith("medication", StringComparison.OrdinalIgnoreCase))
            return ("Medication reminder", "Open app to view");

        return ("New update", "Open app to view");
    }
}
