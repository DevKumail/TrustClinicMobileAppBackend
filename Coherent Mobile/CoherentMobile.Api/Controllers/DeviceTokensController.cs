using System.Security.Claims;
using CoherentMobile.Application.DTOs;
using CoherentMobile.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CoherentMobile.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DeviceTokensController : ControllerBase
{
    private readonly IDeviceTokenService _deviceTokenService;
    private readonly ILogger<DeviceTokensController> _logger;

    public DeviceTokensController(IDeviceTokenService deviceTokenService, ILogger<DeviceTokensController> logger)
    {
        _deviceTokenService = deviceTokenService;
        _logger = logger;
    }

    [HttpPost]
    public async Task<IActionResult> Upsert([FromBody] DeviceTokenUpsertRequestDto request)
    {
        try
        {
            var (userId, userType) = GetCurrentUser();
            if (userId == 0) return Unauthorized();

            await _deviceTokenService.UpsertAsync(userId, userType, request);
            return Ok(new { success = true });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error upserting device token");
            return StatusCode(500, new { message = "An error occurred while saving device token" });
        }
    }

    [HttpPost("remove")]
    public async Task<IActionResult> Remove([FromBody] DeviceTokenRemoveRequestDto request)
    {
        try
        {
            var (userId, userType) = GetCurrentUser();
            if (userId == 0) return Unauthorized();

            await _deviceTokenService.RemoveAsync(userId, userType, request);
            return Ok(new { success = true });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing device token");
            return StatusCode(500, new { message = "An error occurred while removing device token" });
        }
    }

    private (int userId, string userType) GetCurrentUser()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var userType = User.FindFirst("UserType")?.Value ?? "Patient";

        if (int.TryParse(userIdClaim, out int userId))
            return (userId, userType);

        return (0, userType);
    }
}
