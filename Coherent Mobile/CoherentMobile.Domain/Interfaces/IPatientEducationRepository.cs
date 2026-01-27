using CoherentMobile.Domain.Entities;

namespace CoherentMobile.Domain.Interfaces;

public interface IPatientEducationRepository
{
    Task<IEnumerable<PatientEducation>> GetActiveAsync(int? categoryId, string? search, int? limit);
    Task<PatientEducation?> GetByIdAsync(int educationId);
}
