using CoherentMobile.Domain.Entities;

namespace CoherentMobile.Domain.Interfaces;

/// <summary>
/// Repository interface for Promotion operations
/// </summary>
public interface IPromotionRepository
{
    Task<IEnumerable<Promotion>> GetAllActiveAsync();
}
