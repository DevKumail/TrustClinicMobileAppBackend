using CoherentMobile.Domain.Entities;

namespace CoherentMobile.Domain.Interfaces;

/// <summary>
/// Repository interface for Facility operations
/// </summary>
public interface IFacilityRepository
{
    Task<Facility?> GetByIdAsync(int facilityId);
    Task<IEnumerable<Facility>> GetAllActiveAsync();
}
