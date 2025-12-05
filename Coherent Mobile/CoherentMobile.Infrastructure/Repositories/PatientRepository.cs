using CoherentMobile.Application.Exceptions;
using CoherentMobile.Domain.Entities;
using CoherentMobile.Domain.Interfaces;
using CoherentMobile.Infrastructure.Data;
using Dapper;
using System.Data;
using Microsoft.Data.SqlClient;

namespace CoherentMobile.Infrastructure.Repositories;

/// <summary>
/// Patient repository implementation for Users table
/// </summary>
public class PatientRepository : GenericRepository<Patient>, IPatientRepository
{
    protected override string TableName => "Users";

    public PatientRepository(DapperContext context) : base(context)
    {
    }

    public async Task<Patient?> GetByMRNOAsync(string mrno)
    {
        var query = "SELECT * FROM Users WHERE MRNO = @MRNO AND IsDeleted = 0";
        
        using var connection = _context.CreateConnection();
        return await connection.QueryFirstOrDefaultAsync<Patient>(query, new { MRNO = mrno });
    }

    public async Task<Patient?> GetByEmiratesIdAsync(string emiratesId)
    {
        var query = "SELECT * FROM Users WHERE EmiratesId = @EmiratesId AND IsDeleted = 0";
        
        using var connection = _context.CreateConnection();
        return await connection.QueryFirstOrDefaultAsync<Patient>(query, new { EmiratesId = emiratesId });
    }

    public async Task<Patient?> GetByPassportNumberAsync(string passportNumber)
    {
        var query = "SELECT * FROM Users WHERE PassportNumber = @PassportNumber AND IsDeleted = 0";
        
        using var connection = _context.CreateConnection();
        return await connection.QueryFirstOrDefaultAsync<Patient>(query, new { PassportNumber = passportNumber });
    }

    public async Task<Patient?> GetByIdentityAsync(string? emiratesId, string? passportNumber)
    {
        var parameters = new { EmiratesId = emiratesId, PassportNumber = passportNumber };
        return await ExecuteStoredProcSingleAsync("sp_GetPatientByIdentity", parameters);
    }

    public async Task<Patient?> GetByMobileNumberAsync(string mobileNumber)
    {
        var query = "SELECT * FROM Users WHERE MobileNumber = @MobileNumber AND IsDeleted = 0";
        
        using var connection = _context.CreateConnection();
        return await connection.QueryFirstOrDefaultAsync<Patient>(query, new { MobileNumber = mobileNumber });
    }

    public async Task<Patient?> GetByEmailAsync(string email)
    {
        var query = "SELECT * FROM Users WHERE Email = @Email AND IsDeleted = 0";
        
        using var connection = _context.CreateConnection();
        return await connection.QueryFirstOrDefaultAsync<Patient>(query, new { Email = email });
    }

    public async Task<IEnumerable<Patient>> GetActivePatientsAsync()
    {
        var query = "SELECT * FROM vw_ActivePatients";
        
        using var connection = _context.CreateConnection();
        return await connection.QueryAsync<Patient>(query);
    }

    public async Task<bool> SoftDeleteAsync(int id)
    {
        var query = "UPDATE Users SET IsDeleted = 1, DeletedAt = @DeletedAt, UpdatedAt = @UpdatedAt WHERE Id = @Id";
        
        using var connection = _context.CreateConnection();
        var affected = await connection.ExecuteAsync(query, new { 
            Id = id, 
            DeletedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow 
        });
        
        return affected > 0;
    }

    public async Task<bool> UpdateFailedLoginAttemptsAsync(int patientId)
    {
        var affected = await ExecuteStoredProcNonQueryAsync("sp_UpdateFailedLoginAttempts", new { PatientId = patientId });
        return affected > 0;
    }

    public async Task<bool> ResetFailedLoginAttemptsAsync(int patientId)
    {
        var affected = await ExecuteStoredProcNonQueryAsync("sp_ResetFailedLoginAttempts", new { PatientId = patientId });
        return affected > 0;
    }

    public async Task<bool> MRNOExistsAsync(string mrno)
    {
        var query = "SELECT COUNT(1) FROM Users WHERE MRNO = @MRNO AND IsDeleted = 0";
        
        using var connection = _context.CreateConnection();
        var count = await connection.ExecuteScalarAsync<int>(query, new { MRNO = mrno });
        
        return count > 0;
    }

    public async Task<bool> EmiratesIdExistsAsync(string emiratesId)
    {
        var query = "SELECT COUNT(1) FROM Users WHERE EmiratesId = @EmiratesId AND IsDeleted = 0";
        
        using var connection = _context.CreateConnection();
        var count = await connection.ExecuteScalarAsync<int>(query, new { EmiratesId = emiratesId });
        
        return count > 0;
    }

    public async Task<bool> PassportNumberExistsAsync(string passportNumber)
    {
        var query = "SELECT COUNT(1) FROM Users WHERE PassportNumber = @PassportNumber AND IsDeleted = 0";
        
        using var connection = _context.CreateConnection();
        var count = await connection.ExecuteScalarAsync<int>(query, new { PassportNumber = passportNumber });
        
        return count > 0;
    }
}
