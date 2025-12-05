using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using CoherentMobile.Application.DTOs;
using CoherentMobile.Application.Interfaces;
using CoherentMobile.Domain.Entities;
using CoherentMobile.Domain.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace CoherentMobile.Application.Services;

/// <summary>
/// Authentication service implementation with JWT token generation
/// </summary>
public class AuthService : IAuthService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IConfiguration _configuration;

    public AuthService(IUnitOfWork unitOfWork, IConfiguration configuration)
    {
        _unitOfWork = unitOfWork;
        _configuration = configuration;
    }

    public async Task<AuthResponseDto> RegisterAsync(RegisterUserDto registerDto)
    {
        // Check if email already exists
        if (await _unitOfWork.Users.EmailExistsAsync(registerDto.Email))
        {
            throw new InvalidOperationException("Email already registered");
        }

        // Hash password (In production, use BCrypt or similar)
        var passwordHash = HashPassword(registerDto.Password);

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = registerDto.Email,
            PasswordHash = passwordHash,
            FirstName = registerDto.FirstName,
            LastName = registerDto.LastName,
            PhoneNumber = registerDto.PhoneNumber,
            DateOfBirth = registerDto.DateOfBirth,
            Gender = registerDto.Gender,
            IsEmailVerified = false,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.Users.AddAsync(user);
        await _unitOfWork.CommitAsync();

        var token = GenerateJwtToken(user.Id, user.Email);
        var expiresAt = DateTime.UtcNow.AddHours(GetTokenExpiryHours());

        return new AuthResponseDto
        {
            UserId = user.Id,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Token = token,
            ExpiresAt = expiresAt
        };
    }

    public async Task<AuthResponseDto> LoginAsync(LoginDto loginDto)
    {
        var user = await _unitOfWork.Users.GetByEmailAsync(loginDto.Email);
        
        if (user == null || !VerifyPassword(loginDto.Password, user.PasswordHash))
        {
            throw new UnauthorizedAccessException("Invalid email or password");
        }

        if (!user.IsActive)
        {
            throw new UnauthorizedAccessException("Account is deactivated");
        }

        var token = GenerateJwtToken(user.Id, user.Email);
        var expiresAt = DateTime.UtcNow.AddHours(GetTokenExpiryHours());

        return new AuthResponseDto
        {
            UserId = user.Id,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Token = token,
            ExpiresAt = expiresAt
        };
    }

    public Task<bool> ValidateTokenAsync(string token)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Secret"] ?? "YourSuperSecretKeyMinimum32Characters!!");
            
            tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = _configuration["Jwt:Issuer"],
                ValidateAudience = true,
                ValidAudience = _configuration["Jwt:Audience"],
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            }, out SecurityToken validatedToken);

            return Task.FromResult(true);
        }
        catch
        {
            return Task.FromResult(false);
        }
    }

    public string GenerateJwtToken(Guid userId, string email)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Secret"] ?? "YourSuperSecretKeyMinimum32Characters!!");
        
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                new Claim(ClaimTypes.Email, email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            }),
            Expires = DateTime.UtcNow.AddHours(GetTokenExpiryHours()),
            Issuer = _configuration["Jwt:Issuer"],
            Audience = _configuration["Jwt:Audience"],
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    private string HashPassword(string password)
    {
        // In production, use BCrypt.Net-Next or similar
        // This is a simplified example
        return Convert.ToBase64String(Encoding.UTF8.GetBytes(password));
    }

    private bool VerifyPassword(string password, string passwordHash)
    {
        // In production, use BCrypt.Net-Next verification
        return HashPassword(password) == passwordHash;
    }

    private int GetTokenExpiryHours()
    {
        return int.TryParse(_configuration["Jwt:ExpiryHours"], out var hours) ? hours : 24;
    }
}
