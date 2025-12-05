using System.Security.Claims;
using CoherentMobile.Application.DTOs;
using CoherentMobile.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CoherentMobile.API.Controllers;

/// <summary>
/// User management controller
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly ILogger<UserController> _logger;

    public UserController(IUserService userService, ILogger<UserController> logger)
    {
        _userService = userService;
        _logger = logger;
    }

    /// <summary>
    /// Get current user's profile
    /// </summary>
    [HttpGet("profile")]
    public async Task<ActionResult<UserProfileDto>> GetProfile()
    {
        var userId = GetCurrentUserId();
        _logger.LogInformation("Fetching profile for user {UserId}", userId);

        var profile = await _userService.GetUserProfileAsync(userId);
        if (profile == null)
        {
            return NotFound(new { error = "User not found" });
        }

        return Ok(profile);
    }

    /// <summary>
    /// Get all users (admin endpoint)
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<UserProfileDto>>> GetAllUsers()
    {
        _logger.LogInformation("Fetching all users");
        var users = await _userService.GetAllUsersAsync();
        return Ok(users);
    }

    /// <summary>
    /// Update user profile
    /// </summary>
    [HttpPut("profile")]
    public async Task<ActionResult> UpdateProfile([FromBody] UserProfileDto profileDto)
    {
        var userId = GetCurrentUserId();
        _logger.LogInformation("Updating profile for user {UserId}", userId);

        var success = await _userService.UpdateUserProfileAsync(userId, profileDto);
        if (!success)
        {
            return NotFound(new { error = "User not found" });
        }

        return Ok(new { message = "Profile updated successfully" });
    }

    /// <summary>
    /// Deactivate user account
    /// </summary>
    [HttpDelete("deactivate")]
    public async Task<ActionResult> DeactivateAccount()
    {
        var userId = GetCurrentUserId();
        _logger.LogInformation("Deactivating account for user {UserId}", userId);

        var success = await _userService.DeactivateUserAsync(userId);
        if (!success)
        {
            return NotFound(new { error = "User not found" });
        }

        return Ok(new { message = "Account deactivated successfully" });
    }

    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.Parse(userIdClaim ?? throw new UnauthorizedAccessException("User ID not found in token"));
    }
}
