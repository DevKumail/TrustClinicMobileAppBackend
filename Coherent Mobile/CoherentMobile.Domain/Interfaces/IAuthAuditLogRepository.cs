using CoherentMobile.Domain.Entities;

namespace CoherentMobile.Domain.Interfaces;

/// <summary>
/// Authentication Audit Log repository interface
/// </summary>
public interface IAuthAuditLogRepository
{
    Task<int> AddAsync(AuthAuditLog log);
    Task<IEnumerable<AuthAuditLog>> GetByPatientIdAsync(int patientId);
    Task<IEnumerable<AuthAuditLog>> GetByMRNOAsync(string mrno);
    Task<IEnumerable<AuthAuditLog>> GetByActionAsync(string action);
    Task<IEnumerable<AuthAuditLog>> GetRecentLogsAsync(int count);
}
