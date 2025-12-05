namespace CoherentMobile.Application.DTOs.Auth;

/// <summary>
/// Reset password response
/// </summary>
public class ResetPasswordResponseDto
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
}
