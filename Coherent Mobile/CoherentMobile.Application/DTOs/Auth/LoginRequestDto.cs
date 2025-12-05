namespace CoherentMobile.Application.DTOs.Auth;

/// <summary>
/// Login request (Emirates ID or Passport Number + Password)
/// </summary>
public class LoginRequestDto
{
    public string Identifier { get; set; } = string.Empty; // Emirates ID or Passport Number
    public string Password { get; set; } = string.Empty;
}
