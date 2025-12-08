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
    private readonly IAppointmentApiClient _appointmentApiClient;
    private readonly ILogger<IntegrationController> _logger;

    public IntegrationController(
        IHealthDataApiClient healthDataApiClient,
        INotificationApiClient notificationApiClient,
        IAppointmentApiClient appointmentApiClient,
        ILogger<IntegrationController> logger)
    {
        _healthDataApiClient = healthDataApiClient;
        _notificationApiClient = notificationApiClient;
        _appointmentApiClient = appointmentApiClient;
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

        // Set the user ID in the request if needed
        // request.UserId = GetCurrentUserId().ToString();
        
        // Use the SendEmailNotificationAsync method which matches the request object
        return Ok(await _notificationApiClient.SendEmailNotificationAsync(request));
    }

    /// <summary>
    /// Get appointments by MRNO from external system
    /// </summary>
    /// <param name="mrno">Medical Record Number</param>
    /// <returns>List of appointments</returns>
    [HttpGet("appointments/{mrno}")]
    public async Task<ActionResult<IEnumerable<Appointment>>> GetAppointmentsByMrno(string mrno)
    {
        _logger.LogInformation("Fetching appointments for MRNO: {Mrno}", mrno);

        try
        {
            var appointments = await _appointmentApiClient.GetAppointmentsByMrnoAsync(mrno);
            return Ok(appointments);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching appointments for MRNO: {Mrno}", mrno);
            return StatusCode(500, new { error = "Failed to fetch appointments. Please try again later." });
        }
    }

    /// <summary>
    /// Get available time slots for a doctor
    /// </summary>
    /// <param name="doctorId">ID of the doctor</param>
    /// <param name="prsnlAlias">Personnel alias of the doctor</param>
    /// <param name="fromDate">Start date for slot search</param>
    /// <param name="toDate">End date for slot search</param>
    /// <returns>List of available time slots</returns>
    [HttpGet("doctors/{doctorId}/slots")]
    public async Task<ActionResult<DoctorSlotsApiResponse>> GetDoctorSlots(
        int doctorId, 
        [FromQuery] string prsnlAlias,
        [FromQuery] DateTime fromDate,
        [FromQuery] DateTime toDate)
    {
        _logger.LogInformation("Getting available slots for doctor {DoctorId}", doctorId);
        
        try
        {
            var result = await _appointmentApiClient.GetAvailableDoctorSlotsAsync(
                doctorId, 
                prsnlAlias,
                fromDate,
                toDate);
                
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting available slots for doctor {DoctorId}", doctorId);
            return StatusCode(500, new { error = "Failed to fetch available slots. Please try again later." });
        }
    }

    private int GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (int.TryParse(userIdClaim, out int userId))
        {
            return userId;
        }
        throw new UnauthorizedAccessException("User ID not found in token");
    }
}
