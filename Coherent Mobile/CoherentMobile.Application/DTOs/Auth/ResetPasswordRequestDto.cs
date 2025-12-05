namespace CoherentMobile.Application.DTOs.Auth;

/// <summary>
/// Reset password request (with token from email)
/// </summary>
public class ResetPasswordRequestDto
{
    public string Identifier { get; set; } = string.Empty; // Emirates ID or Passport
    public string NewPassword { get; set; } = string.Empty;
    public string ConfirmPassword { get; set; } = string.Empty;
}
