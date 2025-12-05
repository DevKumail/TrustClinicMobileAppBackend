namespace CoherentMobile.Application.DTOs.Auth;

/// <summary>
/// Response DTO for password reset
/// </summary>
public class ForgotPasswordResetResponseDto
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
}
