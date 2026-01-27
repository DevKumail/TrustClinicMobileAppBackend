
using CoherentMobile.Application.DTOs.PatientEducation;
using CoherentMobile.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace CoherentMobile.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PatientEducationController : ControllerBase
{
    private readonly IPatientEducationService _service;
    private readonly ILogger<PatientEducationController> _logger;

    public PatientEducationController(IPatientEducationService service, ILogger<PatientEducationController> logger)
    {
        _service = service;
        _logger = logger;
    }

    [HttpGet("categories")]
    [ProducesResponseType(typeof(object), 200)]
    public async Task<IActionResult> GetCategories([FromQuery] bool? generalOnly = null)
    {
        try
        {
            var categories = await _service.GetCategoriesAsync(generalOnly);
            return Ok(new { Success = true, Data = categories });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving education categories");
            return StatusCode(500, new { Success = false, Message = "An error occurred while retrieving categories" });
        }
    }

    [HttpGet("assigned")]
    [ProducesResponseType(typeof(object), 200)]
    public async Task<IActionResult> GetAssigned([FromQuery] string mrno, [FromQuery] int categoryId, [FromQuery] bool includeExpired = false)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(mrno))
                return BadRequest(new { Success = false, Message = "mrno is required" });

            if (categoryId <= 0)
                return BadRequest(new { Success = false, Message = "categoryId is required" });

            var data = await _service.GetAssignedEducationsByMrnoAsync(mrno, categoryId, includeExpired);
            return Ok(new { Success = true, Data = data });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving assigned education for MRNO {MRNO}", mrno);
            return StatusCode(500, new { Success = false, Message = "An error occurred while retrieving education" });
        }
    }
}
