using CoherentMobile.Domain.Entities;
using CoherentMobile.Domain.Interfaces;
using CoherentMobile.Infrastructure.Data;
using Dapper;
using System.Data;

namespace CoherentMobile.Infrastructure.Repositories;

/// <summary>
/// OTP Verification repository implementation
/// </summary>
public class OTPVerificationRepository : GenericRepository<OTPVerification>, IOTPVerificationRepository
{
    protected override string TableName => "OTPVerifications";

    public OTPVerificationRepository(DapperContext context) : base(context)
    {
    }

    public async Task<OTPVerification?> GetLatestByMRNOAsync(string mrno, string otpType)
    {
        var query = @"SELECT TOP 1 * FROM OTPVerifications 
                      WHERE MRNO = @MRNO AND OTPType = @OTPType 
                      ORDER BY CreatedAt DESC";
        
        using var connection = _context.CreateConnection();
        return await connection.QueryFirstOrDefaultAsync<OTPVerification>(query, new { MRNO = mrno, OTPType = otpType });
    }

    public async Task<OTPVerification?> GetLatestByPatientIdAsync(int patientId, string otpType)
    {
        var query = @"SELECT TOP 1 * FROM OTPVerifications 
                      WHERE PatientId = @PatientId AND OTPType = @OTPType 
                      ORDER BY CreatedAt DESC";
        
        using var connection = _context.CreateConnection();
        return await connection.QueryFirstOrDefaultAsync<OTPVerification>(query, new { PatientId = patientId, OTPType = otpType });
    }

    public async Task<bool> VerifyOTPAsync(string otpCode, string? mrno, int? patientId)
    {
        try
        {
            var parameters = new { OTPCode = otpCode, MRNO = mrno, PatientId = patientId };
            
            using var connection = _context.CreateConnection();
            var result = await connection.QueryFirstOrDefaultAsync<dynamic>(
                "sp_VerifyOTP", 
                parameters, 
                commandType: CommandType.StoredProcedure
            );
            
            if (result == null)
                return false;
            
            // Convert dynamic result to int then compare
            int isValidValue = Convert.ToInt32(result.IsValid);
            return isValidValue == 1;
        }
        catch (Microsoft.Data.SqlClient.SqlException ex)
        {
            throw new CoherentMobile.Application.Exceptions.DatabaseException(
                "Failed to verify OTP", ex, "DB_VERIFY_OTP_ERROR");
        }
    }

    public async Task<bool> MarkAsExpiredAsync(int id)
    {
        var query = "UPDATE OTPVerifications SET IsExpired = 1 WHERE Id = @Id";
        
        using var connection = _context.CreateConnection();
        var affected = await connection.ExecuteAsync(query, new { Id = id });
        
        return affected > 0;
    }

    public async Task<bool> IncrementAttemptCountAsync(int id)
    {
        var query = "UPDATE OTPVerifications SET AttemptCount = AttemptCount + 1 WHERE Id = @Id";
        
        using var connection = _context.CreateConnection();
        var affected = await connection.ExecuteAsync(query, new { Id = id });
        
        return affected > 0;
    }

    public async Task<IEnumerable<OTPVerification>> GetExpiredOTPsAsync()
    {
        var query = @"SELECT * FROM OTPVerifications 
                      WHERE ExpiresAt < GETUTCDATE() AND IsExpired = 0";
        
        using var connection = _context.CreateConnection();
        return await connection.QueryAsync<OTPVerification>(query);
    }
}
