namespace CoherentMobile.Application.DTOs.Auth;

/// <summary>
/// Login request (Email + Password)
/// </summary>
public class LoginRequestDto
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
