using CoherentMobile.Domain.Entities;

namespace CoherentMobile.Domain.Interfaces;

/// <summary>
/// Health record repository interface
/// </summary>
public interface IHealthRecordRepository : IRepository<HealthRecord>
{
    Task<IEnumerable<HealthRecord>> GetByUserIdAsync(int userId);
    Task<IEnumerable<HealthRecord>> GetByUserIdAndTypeAsync(int userId, string recordType);
    Task<IEnumerable<HealthRecord>> GetByDateRangeAsync(int userId, DateTime startDate, DateTime endDate);
}
