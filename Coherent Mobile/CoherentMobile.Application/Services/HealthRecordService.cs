using CoherentMobile.Application.DTOs;
using CoherentMobile.Application.Interfaces;
using CoherentMobile.Domain.Entities;
using CoherentMobile.Domain.Interfaces;

namespace CoherentMobile.Application.Services;

/// <summary>
/// Health record service implementation
/// </summary>
public class HealthRecordService : IHealthRecordService
{
    private readonly IUnitOfWork _unitOfWork;

    public HealthRecordService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<HealthRecordDto> CreateHealthRecordAsync(int userId, CreateHealthRecordDto createDto)
    {
        var healthRecord = new HealthRecord
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            RecordType = createDto.RecordType,
            Value = createDto.Value,
            Unit = createDto.Unit,
            RecordedAt = createDto.RecordedAt,
            Notes = createDto.Notes,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.HealthRecords.AddAsync(healthRecord);
        await _unitOfWork.CommitAsync();

        return MapToDto(healthRecord);
    }

    public async Task<IEnumerable<HealthRecordDto>> GetUserHealthRecordsAsync(int userId)
    {
        var records = await _unitOfWork.HealthRecords.GetByUserIdAsync(userId);
        return records.Select(MapToDto);
    }

    public async Task<IEnumerable<HealthRecordDto>> GetHealthRecordsByTypeAsync(int userId, string recordType)
    {
        var records = await _unitOfWork.HealthRecords.GetByUserIdAndTypeAsync(userId, recordType);
        return records.Select(MapToDto);
    }

    public async Task<IEnumerable<HealthRecordDto>> GetHealthRecordsByDateRangeAsync(
        int userId, DateTime startDate, DateTime endDate)
    {
        var records = await _unitOfWork.HealthRecords.GetByDateRangeAsync(userId, startDate, endDate);
        return records.Select(MapToDto);
    }

    public async Task<bool> DeleteHealthRecordAsync(Guid recordId, int userId)
    {
        var record = await _unitOfWork.HealthRecords.GetByIdAsync(recordId);
        
        if (record == null || record.UserId != userId)
            return false;

        await _unitOfWork.HealthRecords.DeleteAsync(recordId);
        await _unitOfWork.CommitAsync();

        return true;
    }

    private HealthRecordDto MapToDto(HealthRecord record)
    {
        return new HealthRecordDto
        {
            Id = record.Id,
            UserId = record.UserId,
            RecordType = record.RecordType,
            Value = record.Value,
            Unit = record.Unit,
            RecordedAt = record.RecordedAt,
            Notes = record.Notes,
            CreatedAt = record.CreatedAt
        };
    }
}
