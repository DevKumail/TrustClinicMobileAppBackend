using CoherentMobile.Application.DTOs.PatientEducation;

namespace CoherentMobile.Application.Interfaces;

public interface IPatientEducationService
{
    Task<IEnumerable<PatientEducationCategoryDto>> GetCategoriesAsync(bool? generalOnly);
    Task<IEnumerable<PatientEducationAssignmentDto>> GetAssignedEducationsByMrnoAsync(string mrno, int categoryId, bool includeExpired);
}
