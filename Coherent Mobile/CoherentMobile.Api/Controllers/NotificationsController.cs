using System.Security.Claims;
using CoherentMobile.Application.DTOs;
using CoherentMobile.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CoherentMobile.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class NotificationsController : ControllerBase
{
    private readonly INotificationService _notificationService;
    private readonly ILogger<NotificationsController> _logger;

    public NotificationsController(INotificationService notificationService, ILogger<NotificationsController> logger)
    {
        _notificationService = notificationService;
        _logger = logger;
    }

    [HttpGet]
    [ProducesResponseType(typeof(NotificationsGetResponseDto), 200)]
    public async Task<IActionResult> Get([FromQuery] DateTime? since, [FromQuery] int limit = 100)
    {
        try
        {
            var (userId, userType) = GetCurrentUser();
            if (userId == 0) return Unauthorized();

            var sinceUtc = since?.ToUniversalTime();
            var response = await _notificationService.GetSinceAsync(userId, userType, sinceUtc, limit);
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching notifications");
            return StatusCode(500, new { message = "An error occurred while fetching notifications" });
        }
    }

    [HttpPost("ack")]
    [ProducesResponseType(typeof(NotificationsAckResponseDto), 200)]
    public async Task<IActionResult> Ack([FromBody] NotificationsAckRequestDto request)
    {
        try
        {
            var (userId, userType) = GetCurrentUser();
            if (userId == 0) return Unauthorized();

            var response = await _notificationService.AckAsync(userId, userType, request);
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error acking notifications");
            return StatusCode(500, new { message = "An error occurred while updating notifications" });
        }
    }

    [HttpPost]
    [ProducesResponseType(typeof(object), 200)]
    public async Task<IActionResult> Create([FromBody] NotificationCreateRequestDto request)
    {
        var (callerUserId, _) = GetCurrentUser();
        if (callerUserId == 0) return Unauthorized();

        try
        {
            var id = await _notificationService.CreateAsync(request);
            return Ok(new { notificationId = id });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating notification");
            return StatusCode(500, new { message = "An error occurred while creating notification" });
        }
    }

    private (int userId, string userType) GetCurrentUser()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var userType = User.FindFirst("UserType")?.Value ?? "Patient";

        if (int.TryParse(userIdClaim, out int userId))
        {
            return (userId, userType);
        }

        return (0, userType);
    }
}
