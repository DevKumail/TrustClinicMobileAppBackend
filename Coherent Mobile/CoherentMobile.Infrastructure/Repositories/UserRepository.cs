using CoherentMobile.Domain.Entities;
using CoherentMobile.Domain.Interfaces;
using CoherentMobile.Infrastructure.Data;
using Dapper;

namespace CoherentMobile.Infrastructure.Repositories;

/// <summary>
/// User repository implementation using Dapper
/// </summary>
public class UserRepository : BaseRepository<User>, IUserRepository
{
    protected override string TableName => "Users";

    public UserRepository(DapperContext context) : base(context)
    {
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        var query = "SELECT * FROM Users WHERE Email = @Email AND IsDeleted = 0";
        
        using var connection = _context.CreateConnection();
        return await connection.QueryFirstOrDefaultAsync<User>(query, new { Email = email });
    }

    public async Task<bool> EmailExistsAsync(string email)
    {
        var query = "SELECT COUNT(1) FROM Users WHERE Email = @Email AND IsDeleted = 0";
        
        using var connection = _context.CreateConnection();
        var count = await connection.ExecuteScalarAsync<int>(query, new { Email = email });
        
        return count > 0;
    }

    public async Task<IEnumerable<User>> GetActiveUsersAsync()
    {
        var query = "SELECT * FROM Users WHERE IsActive = 1 AND IsDeleted = 0";
        
        using var connection = _context.CreateConnection();
        return await connection.QueryAsync<User>(query);
    }
}
