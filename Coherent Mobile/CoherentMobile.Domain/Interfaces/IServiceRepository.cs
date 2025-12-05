using CoherentMobile.Domain.Entities;

namespace CoherentMobile.Domain.Interfaces;

/// <summary>
/// Repository interface for Service operations
/// </summary>
public interface IServiceRepository
{
    Task<IEnumerable<Service>> GetByFacilityIdAsync(int facilityId);
    Task<IEnumerable<SubService>> GetSubServicesByServiceIdAsync(int serviceId);
}
