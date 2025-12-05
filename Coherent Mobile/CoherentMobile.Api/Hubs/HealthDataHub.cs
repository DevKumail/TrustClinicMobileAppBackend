using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace CoherentMobile.API.Hubs;

/// <summary>
/// SignalR hub for real-time health data updates
/// Enables push notifications to connected mobile clients
/// </summary>
[Authorize]
public class HealthDataHub : Hub
{
    private readonly ILogger<HealthDataHub> _logger;

    public HealthDataHub(ILogger<HealthDataHub> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Called when a client connects to the hub
    /// </summary>
    public override async Task OnConnectedAsync()
    {
        var userId = Context.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        
        if (!string.IsNullOrEmpty(userId))
        {
            // Add user to their personal group for targeted messaging
            await Groups.AddToGroupAsync(Context.ConnectionId, $"user_{userId}");
            _logger.LogInformation("User {UserId} connected to HealthDataHub with connection {ConnectionId}", 
                userId, Context.ConnectionId);
        }

        await base.OnConnectedAsync();
    }

    /// <summary>
    /// Called when a client disconnects from the hub
    /// </summary>
    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = Context.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        
        if (!string.IsNullOrEmpty(userId))
        {
            _logger.LogInformation("User {UserId} disconnected from HealthDataHub", userId);
        }

        await base.OnDisconnectedAsync(exception);
    }

    /// <summary>
    /// Client calls this to subscribe to real-time health data updates
    /// </summary>
    public async Task SubscribeToHealthUpdates(string healthDataType)
    {
        var userId = Context.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        
        if (!string.IsNullOrEmpty(userId))
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"healthdata_{healthDataType}");
            _logger.LogInformation("User {UserId} subscribed to {HealthDataType} updates", userId, healthDataType);
        }
    }

    /// <summary>
    /// Send real-time health data update to a specific user
    /// </summary>
    public async Task SendHealthDataUpdate(string userId, object healthData)
    {
        await Clients.Group($"user_{userId}").SendAsync("ReceiveHealthDataUpdate", healthData);
        _logger.LogInformation("Sent health data update to user {UserId}", userId);
    }

    /// <summary>
    /// Broadcast health alert to all connected clients
    /// </summary>
    public async Task BroadcastHealthAlert(string message, string severity)
    {
        await Clients.All.SendAsync("ReceiveHealthAlert", new { Message = message, Severity = severity, Timestamp = DateTime.UtcNow });
        _logger.LogInformation("Broadcasted health alert: {Message} with severity {Severity}", message, severity);
    }
}
