namespace CoherentMobile.Domain.Entities;

/// <summary>
/// Patient entity for fertility clinic signup flow with QR code
/// </summary>
public class Patient
{
    public int Id { get; set; }
    
    // QR Code Retrieved Data
    public string MRNO { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
    
    // Identity Information
    public string EmiratesIdType { get; set; } = string.Empty; // 'Emirates' or 'Passport'
    public string? EmiratesId { get; set; }
    public string? PassportNumber { get; set; }
    
    // Contact Information
    public string MobileNumber { get; set; } = string.Empty;
    public string? Email { get; set; }
    
    // Authentication
    public string PasswordHash { get; set; } = string.Empty;
    public string PasswordSalt { get; set; } = string.Empty;
    
    // Verification Status
    public bool IsMobileVerified { get; set; }
    public bool IsEmailVerified { get; set; }
    public bool IsProfileComplete { get; set; }
    
    // Account Status
    public bool IsActive { get; set; } = true;
    public bool IsLocked { get; set; }
    public int FailedLoginAttempts { get; set; }
    public DateTime? LastLoginAt { get; set; }
    
    // Audit Fields
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    
    // Navigation Properties
    public ICollection<OTPVerification> OTPVerifications { get; set; } = new List<OTPVerification>();
    public ICollection<LoginSession> LoginSessions { get; set; } = new List<LoginSession>();
}
