namespace CoherentMobile.Domain.Entities;

/// <summary>
/// OTP verification entity for signup, login, and password reset flows
/// </summary>
public class OTPVerification
{
    public int Id { get; set; }
    
    // OTP Details
    public string OTPCode { get; set; } = string.Empty;
    public string OTPType { get; set; } = string.Empty; // 'Signup', 'Login', 'ForgotPassword'
    
    // Patient Information
    public string? MRNO { get; set; }
    public int? PatientId { get; set; }
    
    // Delivery Channel
    public string DeliveryChannel { get; set; } = string.Empty; // 'SMS' or 'Email'
    public string RecipientContact { get; set; } = string.Empty;
    
    // OTP Status
    public bool IsVerified { get; set; }
    public bool IsExpired { get; set; }
    public int AttemptCount { get; set; }
    public int MaxAttempts { get; set; } = 3;
    
    // Timing
    public DateTime ExpiresAt { get; set; }
    public DateTime? VerifiedAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation Property
    public Patient? Patient { get; set; }
}
