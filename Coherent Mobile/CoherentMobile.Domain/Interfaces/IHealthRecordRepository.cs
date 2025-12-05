using CoherentMobile.Domain.Entities;

namespace CoherentMobile.Domain.Interfaces;

/// <summary>
/// Health record repository interface
/// </summary>
public interface IHealthRecordRepository : IRepository<HealthRecord>
{
    Task<IEnumerable<HealthRecord>> GetByUserIdAsync(Guid userId);
    Task<IEnumerable<HealthRecord>> GetByUserIdAndTypeAsync(Guid userId, string recordType);
    Task<IEnumerable<HealthRecord>> GetByDateRangeAsync(Guid userId, DateTime startDate, DateTime endDate);
}
