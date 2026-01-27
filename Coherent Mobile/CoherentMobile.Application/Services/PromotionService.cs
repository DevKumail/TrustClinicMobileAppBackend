using CoherentMobile.Application.DTOs.Clinic;
using CoherentMobile.Application.Interfaces;
using CoherentMobile.Domain.Entities;
using CoherentMobile.Domain.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace CoherentMobile.Application.Services;

/// <summary>
/// Service implementation for promotions
/// </summary>
public class PromotionService : IPromotionService
{
    private readonly IPromotionRepository _promotionRepo;
    private readonly IConfiguration _configuration;
    private readonly ILogger<PromotionService> _logger;

    public PromotionService(
        IPromotionRepository promotionRepo,
        IConfiguration configuration,
        ILogger<PromotionService> logger)
    {
        _promotionRepo = promotionRepo;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<IEnumerable<PromotionDto>> GetAllActiveAsync()
    {
        var promotions = await _promotionRepo.GetAllActiveAsync();
        return promotions.Select(MapToDto);
    }

    private PromotionDto MapToDto(Promotion promotion)
    {
        var imageBaseUrl = _configuration["ImageSettings:BaseUrl"] ?? "https://localhost:7162/images";
        var promotionImagesPath = _configuration["ImageSettings:PromotionImagesPath"] ?? "promotions";

        return new PromotionDto
        {
            PromotionId = promotion.PromotionId,
            Title = promotion.Title,
            ArTitle = promotion.ArTitle,
            Description = promotion.Description,
            ArDescription = promotion.ArDescription,
            ImageUrl = !string.IsNullOrEmpty(promotion.ImageFileName)
                ? $"{imageBaseUrl}/{promotionImagesPath}/{promotion.ImageFileName}"
                : string.Empty,
            LinkUrl = promotion.LinkUrl,
            LinkType = promotion.LinkType,
            DisplayOrder = promotion.DisplayOrder
        };
    }
}
