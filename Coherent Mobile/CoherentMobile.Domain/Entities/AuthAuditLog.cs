namespace CoherentMobile.Domain.Entities;

/// <summary>
/// Authentication audit log entity
/// </summary>
public class AuthAuditLog
{
    public int Id { get; set; }
    
    public int? PatientId { get; set; }
    public string? MRNO { get; set; }
    
    // Action Details
    public string Action { get; set; } = string.Empty; // 'QRScan', 'OTPSent', 'OTPVerified', 'SignupComplete', 'Login', 'Logout', 'PasswordReset'
    public string Status { get; set; } = string.Empty; // 'Success', 'Failed', 'Pending'
    public string? Details { get; set; }
    
    // Request Info
    public string? IPAddress { get; set; }
    public string? DeviceInfo { get; set; }
    
    // Timing
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
