namespace CoherentMobile.Application.DTOs.Auth;

/// <summary>
/// Forgot password response
/// </summary>
public class ForgotPasswordResponseDto
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
}
