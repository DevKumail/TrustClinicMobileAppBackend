using CoherentMobile.Application.DTOs.Clinic;

namespace CoherentMobile.Application.Interfaces;

/// <summary>
/// Service interface for promotions
/// </summary>
public interface IPromotionService
{
    Task<IEnumerable<PromotionDto>> GetAllActiveAsync();
}
