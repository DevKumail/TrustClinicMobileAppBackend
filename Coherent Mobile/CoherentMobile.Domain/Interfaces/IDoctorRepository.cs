using CoherentMobile.Domain.Entities;

namespace CoherentMobile.Domain.Interfaces;

/// <summary>
/// Repository interface for Doctor operations
/// </summary>
public interface IDoctorRepository
{
    Task<IEnumerable<Doctor>> GetByFacilityIdAsync(int facilityId);
    Task<Doctor?> GetByIdAsync(int doctorId);
}
