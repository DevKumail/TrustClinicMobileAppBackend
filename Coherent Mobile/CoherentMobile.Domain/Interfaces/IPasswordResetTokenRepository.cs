using CoherentMobile.Domain.Entities;

namespace CoherentMobile.Domain.Interfaces;

/// <summary>
/// Password Reset Token repository interface
/// </summary>
public interface IPasswordResetTokenRepository
{
    Task<PasswordResetToken?> GetByTokenAsync(string token);
    Task<PasswordResetToken?> GetLatestByPatientIdAsync(int patientId);
    Task<int> AddAsync(PasswordResetToken resetToken);
    Task<bool> MarkAsUsedAsync(int id);
    Task<bool> MarkAsExpiredAsync(int id);
    Task<IEnumerable<PasswordResetToken>> GetExpiredTokensAsync();
}
