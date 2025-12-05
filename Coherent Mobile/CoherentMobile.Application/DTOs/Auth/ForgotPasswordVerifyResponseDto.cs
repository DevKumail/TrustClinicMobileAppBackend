namespace CoherentMobile.Application.DTOs.Auth;

/// <summary>
/// Response DTO for forgot password verification
/// </summary>
public class ForgotPasswordVerifyResponseDto
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? DeliveryChannel { get; set; }
    public int ExpiresIn { get; set; } // OTP expiry in seconds
}
