namespace CoherentMobile.Application.DTOs.Auth;

/// <summary>
/// Response after OTP verification
/// </summary>
public class VerifyOTPResponseDto
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? TempToken { get; set; } // Temporary token for profile creation step
}
