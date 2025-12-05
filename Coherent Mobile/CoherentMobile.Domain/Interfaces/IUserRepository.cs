using CoherentMobile.Domain.Entities;

namespace CoherentMobile.Domain.Interfaces;

/// <summary>
/// User-specific repository interface with additional user operations
/// </summary>
public interface IUserRepository : IRepository<User>
{
    Task<User?> GetByEmailAsync(string email);
    Task<bool> EmailExistsAsync(string email);
    Task<IEnumerable<User>> GetActiveUsersAsync();
}
