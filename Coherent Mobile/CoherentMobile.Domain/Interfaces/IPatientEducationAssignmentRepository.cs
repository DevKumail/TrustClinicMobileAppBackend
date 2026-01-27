using CoherentMobile.Domain.Entities;

namespace CoherentMobile.Domain.Interfaces;

public interface IPatientEducationAssignmentRepository
{
    Task<IEnumerable<PatientEducationAssignment>> GetMyActiveAsync(int patientId, bool includeExpired);
    Task<PatientEducationAssignment?> GetByIdAsync(int assignmentId);
    Task<int> AddAsync(PatientEducationAssignment assignment);
    Task<bool> MarkViewedAsync(int assignmentId, DateTime viewedAtUtc);
}
