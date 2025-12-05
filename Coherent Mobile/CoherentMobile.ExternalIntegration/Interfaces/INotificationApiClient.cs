using CoherentMobile.ExternalIntegration.Models;

namespace CoherentMobile.ExternalIntegration.Interfaces;

/// <summary>
/// Interface for external notification service integration
/// </summary>
public interface INotificationApiClient
{
    Task<NotificationResponse> SendEmailNotificationAsync(NotificationRequest request);
    Task<NotificationResponse> SendSmsNotificationAsync(string phoneNumber, string message);
    Task<bool> SendPushNotificationAsync(string deviceToken, string title, string message);
}
