using CoherentMobile.Application.DTOs.Clinic;
using CoherentMobile.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace CoherentMobile.Api.Controllers;

/// <summary>
/// Guest mode controller - No authentication required
/// Provides public clinic information
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class GuestController : ControllerBase
{
    private readonly IClinicInfoService _clinicInfoService;
    private readonly ILogger<GuestController> _logger;

    public GuestController(
        IClinicInfoService clinicInfoService,
        ILogger<GuestController> logger)
    {
        _clinicInfoService = clinicInfoService;
        _logger = logger;
    }

    /// <summary>
    /// Get complete clinic information including doctors and services
    /// Available without authentication for guest users
    /// </summary>
    [HttpGet("clinic-info")]
    [ProducesResponseType(typeof(ClinicInfoDto), 200)]
    public async Task<IActionResult> GetClinicInfo()
    {
        try
        {
            _logger.LogInformation("Guest user requesting clinic information");
            
            var clinicInfo = await _clinicInfoService.GetClinicInfoAsync();
            
            return Ok(clinicInfo);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving clinic information for guest user");
            return StatusCode(500, new { Success = false, Message = "An error occurred while retrieving clinic information" });
        }
    }
}
