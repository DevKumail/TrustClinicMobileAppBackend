using CoherentMobile.Application.DTOs;

namespace CoherentMobile.Application.Interfaces;

/// <summary>
/// Health record service interface
/// </summary>
public interface IHealthRecordService
{
    Task<HealthRecordDto> CreateHealthRecordAsync(int userId, CreateHealthRecordDto createDto);
    Task<IEnumerable<HealthRecordDto>> GetUserHealthRecordsAsync(int userId);
    Task<IEnumerable<HealthRecordDto>> GetHealthRecordsByTypeAsync(int userId, string recordType);
    Task<IEnumerable<HealthRecordDto>> GetHealthRecordsByDateRangeAsync(int userId, DateTime startDate, DateTime endDate);
    Task<bool> DeleteHealthRecordAsync(Guid recordId, int userId);
}
