using System.Net.Http.Json;
using CoherentMobile.ExternalIntegration.Interfaces;
using CoherentMobile.ExternalIntegration.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace CoherentMobile.ExternalIntegration.Clients;

/// <summary>
/// Implementation of notification API client for third-party integration
/// Handles email, SMS, and push notifications through external services
/// </summary>
public class NotificationApiClient : INotificationApiClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<NotificationApiClient> _logger;
    private readonly string _apiBaseUrl;
    private readonly string _apiKey;

    public NotificationApiClient(
        HttpClient httpClient,
        IConfiguration configuration,
        ILogger<NotificationApiClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        _apiBaseUrl = configuration["ExternalApis:NotificationApi:BaseUrl"] ?? "https://api.notifications.example.com";
        _apiKey = configuration["ExternalApis:NotificationApi:ApiKey"] ?? string.Empty;

        // Configure HttpClient
        _httpClient.BaseAddress = new Uri(_apiBaseUrl);
        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");
        _httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
    }

    public async Task<NotificationResponse> SendEmailNotificationAsync(NotificationRequest request)
    {
        try
        {
            _logger.LogInformation("Sending email notification to {Email}", request.RecipientEmail);

            var response = await _httpClient.PostAsJsonAsync("/api/v1/notifications/email", request);
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<NotificationResponse>() ?? 
                        new NotificationResponse { Success = false, Status = "Failed to deserialize response" };

            _logger.LogInformation("Email notification sent successfully to {Email}, MessageId: {MessageId}", 
                request.RecipientEmail, result.MessageId);
            
            return result;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error sending email notification to {Email}", request.RecipientEmail);
            return new NotificationResponse { Success = false, Status = $"HTTP Error: {ex.Message}" };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending email notification to {Email}", request.RecipientEmail);
            return new NotificationResponse { Success = false, Status = $"Error: {ex.Message}" };
        }
    }

    public async Task<NotificationResponse> SendSmsNotificationAsync(string phoneNumber, string message)
    {
        try
        {
            _logger.LogInformation("Sending SMS notification to {PhoneNumber}", phoneNumber);

            var payload = new { PhoneNumber = phoneNumber, Message = message };
            var response = await _httpClient.PostAsJsonAsync("/api/v1/notifications/sms", payload);
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<NotificationResponse>() ?? 
                        new NotificationResponse { Success = false, Status = "Failed to deserialize response" };

            _logger.LogInformation("SMS notification sent successfully to {PhoneNumber}, MessageId: {MessageId}", 
                phoneNumber, result.MessageId);
            
            return result;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error sending SMS notification to {PhoneNumber}", phoneNumber);
            return new NotificationResponse { Success = false, Status = $"HTTP Error: {ex.Message}" };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending SMS notification to {PhoneNumber}", phoneNumber);
            return new NotificationResponse { Success = false, Status = $"Error: {ex.Message}" };
        }
    }

    public async Task<bool> SendPushNotificationAsync(string deviceToken, string title, string message)
    {
        try
        {
            _logger.LogInformation("Sending push notification to device token {DeviceToken}", deviceToken);

            var payload = new { DeviceToken = deviceToken, Title = title, Message = message };
            var response = await _httpClient.PostAsJsonAsync("/api/v1/notifications/push", payload);
            response.EnsureSuccessStatusCode();

            _logger.LogInformation("Push notification sent successfully to device {DeviceToken}", deviceToken);
            return true;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error sending push notification to device {DeviceToken}", deviceToken);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending push notification to device {DeviceToken}", deviceToken);
            return false;
        }
    }
}
