namespace CoherentMobile.Application.DTOs.Auth;

/// <summary>
/// Request DTO for resetting password after OTP verification
/// </summary>
public class ForgotPasswordResetRequestDto
{
    // Can be MRNO, Emirates ID, or Passport Number
    public string Identifier { get; set; } = string.Empty;
    
    public string NewPassword { get; set; } = string.Empty;
    public string ConfirmPassword { get; set; } = string.Empty;
}
