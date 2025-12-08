using System.Security.Claims;
using CoherentMobile.Application.DTOs;
using CoherentMobile.Application.Interfaces;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CoherentMobile.API.Controllers;

/// <summary>
/// Health records management controller
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class HealthRecordController : ControllerBase
{
    private readonly IHealthRecordService _healthRecordService;
    private readonly IValidator<CreateHealthRecordDto> _validator;
    private readonly ILogger<HealthRecordController> _logger;

    public HealthRecordController(
        IHealthRecordService healthRecordService,
        IValidator<CreateHealthRecordDto> validator,
        ILogger<HealthRecordController> logger)
    {
        _healthRecordService = healthRecordService;
        _validator = validator;
        _logger = logger;
    }

    /// <summary>
    /// Create a new health record
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<HealthRecordDto>> CreateHealthRecord([FromBody] CreateHealthRecordDto createDto)
    {
        var userId = GetCurrentUserId();
        _logger.LogInformation("Creating health record for user {UserId}", userId);

        var validationResult = await _validator.ValidateAsync(createDto);
        if (!validationResult.IsValid)
        {
            return BadRequest(validationResult.Errors);
        }

        var record = await _healthRecordService.CreateHealthRecordAsync(userId, createDto);
        return CreatedAtAction(nameof(GetHealthRecords), new { userId }, record);
    }

    /// <summary>
    /// Get all health records for current user
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<HealthRecordDto>>> GetHealthRecords()
    {
        var userId = GetCurrentUserId();
        _logger.LogInformation("Fetching health records for user {UserId}", userId);

        var records = await _healthRecordService.GetUserHealthRecordsAsync(userId);
        return Ok(records);
    }

    /// <summary>
    /// Get health records by type
    /// </summary>
    [HttpGet("type/{recordType}")]
    public async Task<ActionResult<IEnumerable<HealthRecordDto>>> GetHealthRecordsByType(string recordType)
    {
        var userId = GetCurrentUserId();
        _logger.LogInformation("Fetching {RecordType} records for user {UserId}", recordType, userId);

        var records = await _healthRecordService.GetHealthRecordsByTypeAsync(userId, recordType);
        return Ok(records);
    }

    /// <summary>
    /// Get health records by date range
    /// </summary>
    [HttpGet("date-range")]
    public async Task<ActionResult<IEnumerable<HealthRecordDto>>> GetHealthRecordsByDateRange(
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate)
    {
        var userId = GetCurrentUserId();
        _logger.LogInformation("Fetching health records for user {UserId} from {StartDate} to {EndDate}", 
            userId, startDate, endDate);

        var records = await _healthRecordService.GetHealthRecordsByDateRangeAsync(userId, startDate, endDate);
        return Ok(records);
    }

    /// <summary>
    /// Delete a health record
    /// </summary>
    [HttpDelete("{recordId}")]
    public async Task<ActionResult> DeleteHealthRecord(Guid recordId)
    {
        var userId = GetCurrentUserId();
        _logger.LogInformation("Deleting health record {RecordId} for user {UserId}", recordId, userId);

        var success = await _healthRecordService.DeleteHealthRecordAsync(recordId, userId);
        if (!success)
        {
            return NotFound(new { error = "Health record not found" });
        }

        return Ok(new { message = "Health record deleted successfully" });
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
