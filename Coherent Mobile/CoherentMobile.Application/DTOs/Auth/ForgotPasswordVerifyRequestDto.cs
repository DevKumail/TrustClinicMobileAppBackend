namespace CoherentMobile.Application.DTOs.Auth;

/// <summary>
/// Request DTO for forgot password verification (Step 1)
/// </summary>
public class ForgotPasswordVerifyRequestDto
{
    public string MRNO { get; set; } = string.Empty;
    
    // Either Emirates ID or Passport Number
    public string? EmiratesId { get; set; }
    public string? PassportNumber { get; set; }
    
    public string Email { get; set; } = string.Empty;
    public string MobileNumber { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
    
    // Optional: Which channel to send OTP
    public string DeliveryChannel { get; set; } = "SMS"; // SMS or Email
}
