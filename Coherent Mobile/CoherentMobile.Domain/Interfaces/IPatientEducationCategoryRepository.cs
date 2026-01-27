using CoherentMobile.Domain.Entities;

namespace CoherentMobile.Domain.Interfaces;

public interface IPatientEducationCategoryRepository
{
    Task<IEnumerable<PatientEducationCategory>> GetActiveAsync(bool? generalOnly);
    Task<PatientEducationCategory?> GetByIdAsync(int categoryId);
}
