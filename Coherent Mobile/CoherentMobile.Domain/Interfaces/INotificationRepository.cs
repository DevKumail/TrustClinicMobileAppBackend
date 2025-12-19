using CoherentMobile.Domain.Entities;

namespace CoherentMobile.Domain.Interfaces;

public interface INotificationRepository
{
    Task<int> CreateAsync(AppNotification notification);
    Task<IReadOnlyList<AppNotification>> GetSinceAsync(int userId, string userType, DateTime? sinceUtc, int limit);
    Task<int> AckAsync(int userId, string userType, IReadOnlyList<int> notificationIds, bool markRead);
}
