using CoherentMobile.Domain.Entities;

namespace CoherentMobile.Domain.Interfaces;

/// <summary>
/// OTP Verification repository interface
/// </summary>
public interface IOTPVerificationRepository
{
    Task<OTPVerification?> GetByIdAsync(int id);
    Task<OTPVerification?> GetLatestByMRNOAsync(string mrno, string otpType);
    Task<OTPVerification?> GetLatestByPatientIdAsync(int patientId, string otpType);
    Task<int> AddAsync(OTPVerification otp);
    Task<bool> VerifyOTPAsync(string otpCode, string? mrno, int? patientId);
    Task<bool> MarkAsExpiredAsync(int id);
    Task<bool> IncrementAttemptCountAsync(int id);
    Task<IEnumerable<OTPVerification>> GetExpiredOTPsAsync();
}
