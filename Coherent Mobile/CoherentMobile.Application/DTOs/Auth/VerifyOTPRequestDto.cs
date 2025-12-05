namespace CoherentMobile.Application.DTOs.Auth;

/// <summary>
/// Request to verify OTP (Step 3)
/// </summary>
public class VerifyOTPRequestDto
{
    public string MRNO { get; set; } = string.Empty;
    public string OTPCode { get; set; } = string.Empty;
}
