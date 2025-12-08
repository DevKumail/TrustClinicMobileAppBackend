using CoherentMobile.Application.DTOs;

namespace CoherentMobile.Application.Interfaces;

/// <summary>
/// User service interface
/// </summary>
public interface IUserService
{
    Task<UserProfileDto?> GetUserProfileAsync(int userId);
    Task<IEnumerable<UserProfileDto>> GetAllUsersAsync();
    Task<bool> UpdateUserProfileAsync(int userId, UserProfileDto profileDto);
    Task<bool> DeactivateUserAsync(int userId);
}
