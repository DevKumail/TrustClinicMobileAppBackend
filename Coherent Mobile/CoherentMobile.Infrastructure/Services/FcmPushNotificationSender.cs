using CoherentMobile.Application.Interfaces;
using CoherentMobile.Domain.Interfaces;
using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Google.Apis.Auth.OAuth2;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace CoherentMobile.Infrastructure.Services;

public class FcmPushNotificationSender : IPushNotificationSender
{
    private readonly IDeviceTokenRepository _deviceTokens;
    private readonly ILogger<FcmPushNotificationSender> _logger;
    private readonly string? _serviceAccountJsonPath;
    private readonly string? _serviceAccountJson;
    private bool _enabled = true;

    public FcmPushNotificationSender(IDeviceTokenRepository deviceTokens, IConfiguration configuration, ILogger<FcmPushNotificationSender> logger)
    {
        _deviceTokens = deviceTokens;
        _logger = logger;
        _serviceAccountJsonPath = configuration["Firebase:ServiceAccountJsonPath"];
        _serviceAccountJson = configuration["Firebase:ServiceAccountJson"];

        try
        {
            EnsureFirebaseInitialized();
        }
        catch (Exception ex)
        {
            _enabled = false;
            _logger.LogWarning(ex, "FCM is disabled because Firebase initialization failed");
        }
    }

    private void EnsureFirebaseInitialized()
    {
        if (FirebaseApp.DefaultInstance != null)
            return;

        GoogleCredential credential;

        if (!string.IsNullOrWhiteSpace(_serviceAccountJsonPath))
        {
            credential = GoogleCredential.FromFile(_serviceAccountJsonPath);
        }
        else if (!string.IsNullOrWhiteSpace(_serviceAccountJson))
        {
            credential = GoogleCredential.FromJson(_serviceAccountJson);
        }
        else
        {
            throw new InvalidOperationException("Firebase service account is not configured. Set Firebase:ServiceAccountJsonPath or Firebase:ServiceAccountJson.");
        }

        FirebaseApp.Create(new AppOptions { Credential = credential });
    }

    public async Task SendWakeupAsync(int userId, string userType, string? title, string? body, IReadOnlyDictionary<string, string>? data = null)
    {
        if (!_enabled)
            return;

        var tokens = await _deviceTokens.GetActiveAsync(userId, userType);
        if (tokens.Count == 0)
            return;

        var messageData = data == null ? new Dictionary<string, string>() : new Dictionary<string, string>(data);
        messageData.TryAdd("event", "notifications_changed");
        
        // Include title and body in data payload for Flutter background handler
        messageData.TryAdd("title", string.IsNullOrWhiteSpace(title) ? "New update" : title);
        messageData.TryAdd("body", string.IsNullOrWhiteSpace(body) ? "Open app to view" : body);

        // Include Notification payload for kill mode - Android system will display it
        // Data payload is also included for app to process when opened
        var displayTitle = string.IsNullOrWhiteSpace(title) ? "Medication Reminder" : title;
        var displayBody = string.IsNullOrWhiteSpace(body) ? "Time for your medication" : body;

        var multicast = new MulticastMessage
        {
            Tokens = tokens.Select(t => t.Token).ToList(),
            Data = messageData,
            Notification = new Notification
            {
                Title = displayTitle,
                Body = displayBody
            },
            Android = new AndroidConfig
            {
                Priority = Priority.High,
                Notification = new AndroidNotification
                {
                    Title = displayTitle,
                    Body = displayBody,
                    ChannelId = "medication_reminder_channel",
                    Priority = NotificationPriority.MAX,
                    DefaultSound = true,
                    DefaultVibrateTimings = true
                }
            },
            Apns = new ApnsConfig
            {
                Aps = new Aps
                {
                    Alert = new ApsAlert
                    {
                        Title = displayTitle,
                        Body = displayBody
                    },
                    Sound = "default",
                    ContentAvailable = true
                },
                Headers = new Dictionary<string, string>
                {
                    ["apns-priority"] = "10"
                }
            }
        };

        try
        {
            var response = await FirebaseMessaging.DefaultInstance.SendEachForMulticastAsync(multicast);
            if (response.FailureCount > 0)
            {
                for (var i = 0; i < response.Responses.Count; i++)
                {
                    var r = response.Responses[i];
                    if (r.IsSuccess)
                        continue;

                    var token = multicast.Tokens[i];
                    _logger.LogWarning(r.Exception, "FCM send failed for token {Token}", token);

                    // Deactivate invalid/expired tokens
                    if (IsTokenInvalid(r.Exception))
                    {
                        _logger.LogInformation("Deactivating invalid FCM token {Token} for user {UserId}", token, userId);
                        await _deviceTokens.DeactivateAsync(userId, userType, token);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "FCM send failed");
        }
    }

    private static bool IsTokenInvalid(Exception? ex)
    {
        if (ex is not FirebaseMessagingException fme)
            return false;

        // These error codes indicate the token is permanently invalid
        return fme.MessagingErrorCode is 
            MessagingErrorCode.Unregistered or      // App uninstalled or token expired
            MessagingErrorCode.InvalidArgument or   // Invalid token format
            MessagingErrorCode.SenderIdMismatch;    // Token from different Firebase project
    }
}
