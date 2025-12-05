using CoherentMobile.Application.DTOs;
using CoherentMobile.Application.Interfaces;
using CoherentMobile.Domain.Interfaces;

namespace CoherentMobile.Application.Services;

/// <summary>
/// User service implementation
/// </summary>
public class UserService : IUserService
{
    private readonly IUnitOfWork _unitOfWork;

    public UserService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<UserProfileDto?> GetUserProfileAsync(Guid userId)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(userId);
        
        if (user == null)
            return null;

        return new UserProfileDto
        {
            Id = user.Id,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            PhoneNumber = user.PhoneNumber,
            DateOfBirth = user.DateOfBirth,
            Gender = user.Gender,
            IsEmailVerified = user.IsEmailVerified
        };
    }

    public async Task<IEnumerable<UserProfileDto>> GetAllUsersAsync()
    {
        var users = await _unitOfWork.Users.GetActiveUsersAsync();
        
        return users.Select(u => new UserProfileDto
        {
            Id = u.Id,
            Email = u.Email,
            FirstName = u.FirstName,
            LastName = u.LastName,
            PhoneNumber = u.PhoneNumber,
            DateOfBirth = u.DateOfBirth,
            Gender = u.Gender,
            IsEmailVerified = u.IsEmailVerified
        });
    }

    public async Task<bool> UpdateUserProfileAsync(Guid userId, UserProfileDto profileDto)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(userId);
        
        if (user == null)
            return false;

        user.FirstName = profileDto.FirstName;
        user.LastName = profileDto.LastName;
        user.PhoneNumber = profileDto.PhoneNumber;
        user.DateOfBirth = profileDto.DateOfBirth;
        user.Gender = profileDto.Gender;
        user.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.Users.UpdateAsync(user);
        await _unitOfWork.CommitAsync();

        return true;
    }

    public async Task<bool> DeactivateUserAsync(Guid userId)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(userId);
        
        if (user == null)
            return false;

        user.IsActive = false;
        user.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.Users.UpdateAsync(user);
        await _unitOfWork.CommitAsync();

        return true;
    }
}
