using CoherentMobile.Application.DTOs;

namespace CoherentMobile.Application.Interfaces;

/// <summary>
/// User service interface
/// </summary>
public interface IUserService
{
    Task<UserProfileDto?> GetUserProfileAsync(Guid userId);
    Task<IEnumerable<UserProfileDto>> GetAllUsersAsync();
    Task<bool> UpdateUserProfileAsync(Guid userId, UserProfileDto profileDto);
    Task<bool> DeactivateUserAsync(Guid userId);
}
