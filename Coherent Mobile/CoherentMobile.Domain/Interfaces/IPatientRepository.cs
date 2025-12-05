using CoherentMobile.Domain.Entities;

namespace CoherentMobile.Domain.Interfaces;

/// <summary>
/// Patient repository interface for user authentication operations
/// </summary>
public interface IPatientRepository
{
    Task<Patient?> GetByIdAsync(int id);
    Task<Patient?> GetByMRNOAsync(string mrno);
    Task<Patient?> GetByEmiratesIdAsync(string emiratesId);
    Task<Patient?> GetByPassportNumberAsync(string passportNumber);
    Task<Patient?> GetByIdentityAsync(string? emiratesId, string? passportNumber);
    Task<Patient?> GetByMobileNumberAsync(string mobileNumber);
    Task<Patient?> GetByEmailAsync(string email);
    Task<IEnumerable<Patient>> GetActivePatientsAsync();
    Task<int> AddAsync(Patient patient);
    Task<bool> UpdateAsync(Patient patient);
    Task<bool> SoftDeleteAsync(int id);
    Task<bool> UpdateFailedLoginAttemptsAsync(int patientId);
    Task<bool> ResetFailedLoginAttemptsAsync(int patientId);
    Task<bool> MRNOExistsAsync(string mrno);
    Task<bool> EmiratesIdExistsAsync(string emiratesId);
    Task<bool> PassportNumberExistsAsync(string passportNumber);
}
