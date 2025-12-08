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

    public async Task<UserProfileDto?> GetUserProfileAsync(int userId)
    {
        var user = await _unitOfWork.Patients.GetByIdAsync(userId);
        
        if (user == null)
            return null;

        return new UserProfileDto
        {
            Id = user.Id,
            Email = user.Email ?? string.Empty,
            FirstName = user.FullName.Split(' ').FirstOrDefault() ?? string.Empty,
            LastName = user.FullName.Split(' ').Skip(1).FirstOrDefault() ?? string.Empty,
            PhoneNumber = user.MobileNumber,
            DateOfBirth = user.DateOfBirth,
            Gender = string.Empty,
            IsEmailVerified = user.IsEmailVerified
        };
    }

    public async Task<IEnumerable<UserProfileDto>> GetAllUsersAsync()
    {
        var users = await _unitOfWork.Patients.GetActivePatientsAsync();
        
        return users.Select(u => new UserProfileDto
        {
            Id = u.Id,
            Email = u.Email ?? string.Empty,
            FirstName = u.FullName.Split(' ').FirstOrDefault() ?? string.Empty,
            LastName = u.FullName.Split(' ').Skip(1).FirstOrDefault() ?? string.Empty,
            PhoneNumber = u.MobileNumber,
            DateOfBirth = u.DateOfBirth,
            Gender = string.Empty,
            IsEmailVerified = u.IsEmailVerified
        });
    }

    public async Task<bool> UpdateUserProfileAsync(int userId, UserProfileDto profileDto)
    {
        var user = await _unitOfWork.Patients.GetByIdAsync(userId);
        
        if (user == null)
            return false;

        user.FullName = $"{profileDto.FirstName} {profileDto.LastName}".Trim();
        user.MobileNumber = profileDto.PhoneNumber;
        user.DateOfBirth = profileDto.DateOfBirth ?? DateTime.UtcNow;
        user.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.Patients.UpdateAsync(user);
        await _unitOfWork.CommitAsync();

        return true;
    }

    public async Task<bool> DeactivateUserAsync(int userId)
    {
        var user = await _unitOfWork.Patients.GetByIdAsync(userId);
        
        if (user == null)
            return false;

        user.IsActive = false;
        user.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.Patients.UpdateAsync(user);
        await _unitOfWork.CommitAsync();

        return true;
    }
}
