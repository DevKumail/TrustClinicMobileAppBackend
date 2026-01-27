using CoherentMobile.Application.DTOs.Clinic;
using CoherentMobile.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace CoherentMobile.Api.Controllers;

/// <summary>
/// Promotions controller - Public API for mobile app banners/promotions
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class PromotionsController : ControllerBase
{
    private readonly IPromotionService _promotionService;
    private readonly ILogger<PromotionsController> _logger;

    public PromotionsController(
        IPromotionService promotionService,
        ILogger<PromotionsController> logger)
    {
        _promotionService = promotionService;
        _logger = logger;
    }

    /// <summary>
    /// Get all active promotions (IsActive = true, IsDeleted = false)
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<PromotionDto>), 200)]
    public async Task<IActionResult> GetAllActivePromotions()
    {
        try
        {
            var promotions = await _promotionService.GetAllActiveAsync();
            
            return Ok(new 
            { 
                Success = true, 
                Data = promotions,
                Message = "Promotions retrieved successfully"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving promotions");
            return StatusCode(500, new { Success = false, Message = "An error occurred while retrieving promotions" });
        }
    }
}
