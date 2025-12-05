using CoherentMobile.Domain.Entities;

namespace CoherentMobile.Domain.Interfaces;

/// <summary>
/// Repository interface for Speciality operations
/// </summary>
public interface ISpecialityRepository
{
    Task<Speciality?> GetByIdAsync(int specialityId);
}
