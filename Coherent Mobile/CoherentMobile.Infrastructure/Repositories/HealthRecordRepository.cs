using CoherentMobile.Domain.Entities;
using CoherentMobile.Domain.Interfaces;
using CoherentMobile.Infrastructure.Data;
using Dapper;

namespace CoherentMobile.Infrastructure.Repositories;

/// <summary>
/// Health record repository implementation using Dapper
/// </summary>
public class HealthRecordRepository : BaseRepository<HealthRecord>, IHealthRecordRepository
{
    protected override string TableName => "HealthRecords";

    public HealthRecordRepository(DapperContext context) : base(context)
    {
    }

    public async Task<IEnumerable<HealthRecord>> GetByUserIdAsync(int userId)
    {
        var query = "SELECT * FROM HealthRecords WHERE UserId = @UserId AND IsDeleted = 0 ORDER BY RecordedAt DESC";
        
        using var connection = _context.CreateConnection();
        return await connection.QueryAsync<HealthRecord>(query, new { UserId = userId });
    }

    public async Task<IEnumerable<HealthRecord>> GetByUserIdAndTypeAsync(int userId, string recordType)
    {
        var query = @"SELECT * FROM HealthRecords 
                     WHERE UserId = @UserId 
                     AND RecordType = @RecordType 
                     AND IsDeleted = 0 
                     ORDER BY RecordedAt DESC";
        
        using var connection = _context.CreateConnection();
        return await connection.QueryAsync<HealthRecord>(query, new { UserId = userId, RecordType = recordType });
    }

    public async Task<IEnumerable<HealthRecord>> GetByDateRangeAsync(int userId, DateTime startDate, DateTime endDate)
    {
        var query = @"SELECT * FROM HealthRecords 
                     WHERE UserId = @UserId 
                     AND RecordedAt BETWEEN @StartDate AND @EndDate 
                     AND IsDeleted = 0 
                     ORDER BY RecordedAt DESC";
        
        using var connection = _context.CreateConnection();
        return await connection.QueryAsync<HealthRecord>(query, new { UserId = userId, StartDate = startDate, EndDate = endDate });
    }
}
