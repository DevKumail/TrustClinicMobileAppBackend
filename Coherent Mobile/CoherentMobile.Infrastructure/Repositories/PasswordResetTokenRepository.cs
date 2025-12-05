using CoherentMobile.Domain.Entities;
using CoherentMobile.Domain.Interfaces;
using CoherentMobile.Infrastructure.Data;
using Dapper;

namespace CoherentMobile.Infrastructure.Repositories;

/// <summary>
/// Password Reset Token repository implementation
/// </summary>
public class PasswordResetTokenRepository : GenericRepository<PasswordResetToken>, IPasswordResetTokenRepository
{
    protected override string TableName => "PasswordResetTokens";

    public PasswordResetTokenRepository(DapperContext context) : base(context)
    {
    }

    public async Task<PasswordResetToken?> GetByTokenAsync(string token)
    {
        var query = @"SELECT * FROM PasswordResetTokens 
                      WHERE Token = @Token 
                      AND IsUsed = 0 
                      AND IsExpired = 0 
                      AND ExpiresAt > GETUTCDATE()";
        
        using var connection = _context.CreateConnection();
        return await connection.QueryFirstOrDefaultAsync<PasswordResetToken>(query, new { Token = token });
    }

    public async Task<PasswordResetToken?> GetLatestByPatientIdAsync(int patientId)
    {
        var query = @"SELECT TOP 1 * FROM PasswordResetTokens 
                      WHERE PatientId = @PatientId 
                      ORDER BY CreatedAt DESC";
        
        using var connection = _context.CreateConnection();
        return await connection.QueryFirstOrDefaultAsync<PasswordResetToken>(query, new { PatientId = patientId });
    }

    public async Task<bool> MarkAsUsedAsync(int id)
    {
        var query = "UPDATE PasswordResetTokens SET IsUsed = 1, UsedAt = @UsedAt WHERE Id = @Id";
        
        using var connection = _context.CreateConnection();
        var affected = await connection.ExecuteAsync(query, new { Id = id, UsedAt = DateTime.UtcNow });
        
        return affected > 0;
    }

    public async Task<bool> MarkAsExpiredAsync(int id)
    {
        var query = "UPDATE PasswordResetTokens SET IsExpired = 1 WHERE Id = @Id";
        
        using var connection = _context.CreateConnection();
        var affected = await connection.ExecuteAsync(query, new { Id = id });
        
        return affected > 0;
    }

    public async Task<IEnumerable<PasswordResetToken>> GetExpiredTokensAsync()
    {
        var query = @"SELECT * FROM PasswordResetTokens 
                      WHERE ExpiresAt < GETUTCDATE() 
                      AND IsExpired = 0";
        
        using var connection = _context.CreateConnection();
        return await connection.QueryAsync<PasswordResetToken>(query);
    }
}
