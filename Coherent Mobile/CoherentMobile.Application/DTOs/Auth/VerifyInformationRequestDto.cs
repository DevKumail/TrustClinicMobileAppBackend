namespace CoherentMobile.Application.DTOs.Auth;

/// <summary>
/// Request to verify user information (Step 2 after QR scan)
/// </summary>
public class VerifyInformationRequestDto
{
    public string MRNO { get; set; } = string.Empty;
    
    // For Emirates ID Type
    public string? EmiratesId { get; set; }
    
    // For Passport Type
    public string? PassportNumber { get; set; }
    public string? Email { get; set; }
    
    // Common for both
    public string MobileNumber { get; set; } = string.Empty;
    
    // OTP delivery preference
    public string DeliveryChannel { get; set; } = "SMS"; // "SMS" or "Email"
}
