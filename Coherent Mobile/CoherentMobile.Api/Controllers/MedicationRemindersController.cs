using System.Security.Claims;
using CoherentMobile.Application.DTOs;
using CoherentMobile.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CoherentMobile.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class MedicationRemindersController : ControllerBase
{
    private readonly IMedicationReminderService _service;
    private readonly ILogger<MedicationRemindersController> _logger;

    public MedicationRemindersController(IMedicationReminderService service, ILogger<MedicationRemindersController> logger)
    {
        _service = service;
        _logger = logger;
    }

    [HttpGet]
    [ProducesResponseType(typeof(List<MedicationReminderDto>), 200)]
    public async Task<IActionResult> GetMy([FromQuery] bool activeOnly = true)
    {
        try
        {
            var (userId, userType) = GetCurrentUser();
            if (userId == 0) return Unauthorized();

            var result = await _service.GetMyAsync(userId, userType, activeOnly);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching medication reminders");
            return StatusCode(500, new { message = "An error occurred while fetching medication reminders" });
        }
    }

    [HttpPost]
    [ProducesResponseType(typeof(object), 200)]
    public async Task<IActionResult> Create([FromBody] MedicationReminderUpsertRequestDto request)
    {
        try
        {
            var (userId, userType) = GetCurrentUser();
            if (userId == 0) return Unauthorized();

            var id = await _service.CreateAsync(userId, userType, request);
            return Ok(new { medicationReminderId = id });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating medication reminder");
            return StatusCode(500, new { message = "An error occurred while creating medication reminder" });
        }
    }

    [HttpPut("{medicationReminderId:int}")]
    [ProducesResponseType(typeof(object), 200)]
    public async Task<IActionResult> Update(int medicationReminderId, [FromBody] MedicationReminderUpsertRequestDto request)
    {
        try
        {
            var (userId, userType) = GetCurrentUser();
            if (userId == 0) return Unauthorized();

            var ok = await _service.UpdateAsync(userId, userType, medicationReminderId, request);
            if (!ok) return NotFound(new { message = "Medication reminder not found" });

            return Ok(new { success = true });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating medication reminder");
            return StatusCode(500, new { message = "An error occurred while updating medication reminder" });
        }
    }

    [HttpDelete("{medicationReminderId:int}")]
    [ProducesResponseType(typeof(object), 200)]
    public async Task<IActionResult> Deactivate(int medicationReminderId)
    {
        try
        {
            var (userId, userType) = GetCurrentUser();
            if (userId == 0) return Unauthorized();

            var ok = await _service.DeactivateAsync(userId, userType, medicationReminderId);
            if (!ok) return NotFound(new { message = "Medication reminder not found" });

            return Ok(new { success = true });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deactivating medication reminder");
            return StatusCode(500, new { message = "An error occurred while deactivating medication reminder" });
        }
    }

    [HttpPost("{medicationReminderId:int}/action")]
    [ProducesResponseType(typeof(object), 200)]
    public async Task<IActionResult> ApplyAction(int medicationReminderId, [FromBody] MedicationReminderActionRequestDto request)
    {
        try
        {
            var (userId, userType) = GetCurrentUser();
            if (userId == 0) return Unauthorized();

            var ok = await _service.ApplyActionAsync(userId, userType, medicationReminderId, request);
            if (!ok) return NotFound(new { message = "Medication reminder not found" });

            return Ok(new { success = true });
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid medication reminder action");
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error applying medication reminder action");
            return StatusCode(500, new { message = "An error occurred while applying medication reminder action" });
        }
    }

    private (int userId, string userType) GetCurrentUser()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var userType = User.FindFirst("UserType")?.Value ?? "Patient";

        if (int.TryParse(userIdClaim, out var userId))
            return (userId, userType);

        return (0, userType);
    }
}
