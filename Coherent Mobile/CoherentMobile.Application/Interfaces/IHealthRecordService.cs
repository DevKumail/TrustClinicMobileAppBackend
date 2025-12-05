using CoherentMobile.Application.DTOs;

namespace CoherentMobile.Application.Interfaces;

/// <summary>
/// Health record service interface
/// </summary>
public interface IHealthRecordService
{
    Task<HealthRecordDto> CreateHealthRecordAsync(Guid userId, CreateHealthRecordDto createDto);
    Task<IEnumerable<HealthRecordDto>> GetUserHealthRecordsAsync(Guid userId);
    Task<IEnumerable<HealthRecordDto>> GetHealthRecordsByTypeAsync(Guid userId, string recordType);
    Task<IEnumerable<HealthRecordDto>> GetHealthRecordsByDateRangeAsync(Guid userId, DateTime startDate, DateTime endDate);
    Task<bool> DeleteHealthRecordAsync(Guid recordId, Guid userId);
}
