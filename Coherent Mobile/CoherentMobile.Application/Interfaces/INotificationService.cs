using CoherentMobile.Application.DTOs;

namespace CoherentMobile.Application.Interfaces;

public interface INotificationService
{
    Task<NotificationsGetResponseDto> GetSinceAsync(int userId, string userType, DateTime? sinceUtc, int limit);
    Task<NotificationsAckResponseDto> AckAsync(int userId, string userType, NotificationsAckRequestDto request);
    Task<int> CreateAsync(NotificationCreateRequestDto request);
}
