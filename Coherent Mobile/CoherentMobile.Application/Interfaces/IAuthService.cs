using CoherentMobile.Application.DTOs;

namespace CoherentMobile.Application.Interfaces;

/// <summary>
/// Authentication service interface
/// </summary>
public interface IAuthService
{
    Task<AuthResponseDto> RegisterAsync(RegisterUserDto registerDto);
    Task<AuthResponseDto> LoginAsync(LoginDto loginDto);
    Task<bool> ValidateTokenAsync(string token);
    string GenerateJwtToken(int userId, string email);
}
