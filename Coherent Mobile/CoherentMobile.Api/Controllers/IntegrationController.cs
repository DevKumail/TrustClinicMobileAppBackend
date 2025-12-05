using System.Security.Claims;
using CoherentMobile.ExternalIntegration.Interfaces;
using CoherentMobile.ExternalIntegration.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CoherentMobile.API.Controllers;

/// <summary>
/// Controller demonstrating external API integration
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class IntegrationController : ControllerBase
{
    private readonly IHealthDataApiClient _healthDataApiClient;
    private readonly INotificationApiClient _notificationApiClient;
    private readonly ILogger<IntegrationController> _logger;

    public IntegrationController(
        IHealthDataApiClient healthDataApiClient,
        INotificationApiClient notificationApiClient,
        ILogger<IntegrationController> logger)
    {
        _healthDataApiClient = healthDataApiClient;
        _notificationApiClient = notificationApiClient;
        _logger = logger;
    }

    /// <summary>
    /// Fetch external health data
    /// </summary>
    [HttpGet("external-health-data/{dataType}")]
    public async Task<ActionResult<ExternalHealthDataResponse>> GetExternalHealthData(string dataType)
    {
        var userId = GetCurrentUserId().ToString();
        _logger.LogInformation("Fetching external health data of type {DataType} for user {UserId}", dataType, userId);

        var data = await _healthDataApiClient.GetHealthDataAsync(userId, dataType);
        if (data == null)
        {
            return NotFound(new { error = "External health data not found" });
        }

        return Ok(data);
    }

    /// <summary>
    /// Sync health data to external API
    /// </summary>
    [HttpPost("sync-health-data")]
    public async Task<ActionResult> SyncHealthData([FromBody] HealthData healthData)
    {
        var userId = GetCurrentUserId().ToString();
        _logger.LogInformation("Syncing health data for user {UserId}", userId);

        var success = await _healthDataApiClient.SyncHealthDataAsync(userId, healthData);
        if (!success)
        {
            return StatusCode(500, new { error = "Failed to sync health data" });
        }

        return Ok(new { message = "Health data synced successfully" });
    }

    /// <summary>
    /// Send notification via external service
    /// </summary>
    [HttpPost("send-notification")]
    public async Task<ActionResult<NotificationResponse>> SendNotification([FromBody] NotificationRequest request)
    {
        _logger.LogInformation("Sending notification to {Email}", request.RecipientEmail);

        var response = await _notificationApiClient.SendEmailNotificationAsync(request);
        if (!response.Success)
        {
            return StatusCode(500, new { error = "Failed to send notification", details = response.Status });
        }

        return Ok(response);
    }

    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.Parse(userIdClaim ?? throw new UnauthorizedAccessException("User ID not found in token"));
    }
}
