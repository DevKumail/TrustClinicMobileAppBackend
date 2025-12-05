using CoherentMobile.Domain.Entities;
using CoherentMobile.Domain.Interfaces;
using CoherentMobile.Infrastructure.Data;
using Dapper;

namespace CoherentMobile.Infrastructure.Repositories;

/// <summary>
/// Authentication Audit Log repository implementation
/// </summary>
public class AuthAuditLogRepository : GenericRepository<AuthAuditLog>, IAuthAuditLogRepository
{
    protected override string TableName => "AuthAuditLogs";

    public AuthAuditLogRepository(DapperContext context) : base(context)
    {
    }

    public async Task<IEnumerable<AuthAuditLog>> GetByPatientIdAsync(int patientId)
    {
        var query = "SELECT * FROM AuthAuditLogs WHERE PatientId = @PatientId ORDER BY CreatedAt DESC";
        
        using var connection = _context.CreateConnection();
        return await connection.QueryAsync<AuthAuditLog>(query, new { PatientId = patientId });
    }

    public async Task<IEnumerable<AuthAuditLog>> GetByMRNOAsync(string mrno)
    {
        var query = "SELECT * FROM AuthAuditLogs WHERE MRNO = @MRNO ORDER BY CreatedAt DESC";
        
        using var connection = _context.CreateConnection();
        return await connection.QueryAsync<AuthAuditLog>(query, new { MRNO = mrno });
    }

    public async Task<IEnumerable<AuthAuditLog>> GetByActionAsync(string action)
    {
        var query = "SELECT * FROM AuthAuditLogs WHERE Action = @Action ORDER BY CreatedAt DESC";
        
        using var connection = _context.CreateConnection();
        return await connection.QueryAsync<AuthAuditLog>(query, new { Action = action });
    }

    public async Task<IEnumerable<AuthAuditLog>> GetRecentLogsAsync(int count)
    {
        var query = $"SELECT TOP {count} * FROM AuthAuditLogs ORDER BY CreatedAt DESC";
        
        using var connection = _context.CreateConnection();
        return await connection.QueryAsync<AuthAuditLog>(query);
    }
}
