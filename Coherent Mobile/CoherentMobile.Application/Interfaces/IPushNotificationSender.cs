namespace CoherentMobile.Application.Interfaces;

public interface IPushNotificationSender
{
    Task SendWakeupAsync(int userId, string userType, string? title, string? body, IReadOnlyDictionary<string, string>? data = null);
}
