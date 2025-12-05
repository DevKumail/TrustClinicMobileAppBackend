namespace CoherentMobile.Application.DTOs.Auth;

/// <summary>
/// Request to create profile and set password (Step 4)
/// </summary>
public class CreateProfileRequestDto
{
    public string MRNO { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string ConfirmPassword { get; set; } = string.Empty;
}
