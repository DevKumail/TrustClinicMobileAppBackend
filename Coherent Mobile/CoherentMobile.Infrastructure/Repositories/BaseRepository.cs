using System.Data;
using CoherentMobile.Domain.Entities;
using CoherentMobile.Domain.Interfaces;
using CoherentMobile.Infrastructure.Data;
using Dapper;

namespace CoherentMobile.Infrastructure.Repositories;

/// <summary>
/// Base repository implementation using Dapper
/// </summary>
/// <typeparam name="T">Entity type</typeparam>
public abstract class BaseRepository<T> : IRepository<T> where T : BaseEntity
{
    protected readonly DapperContext _context;
    protected abstract string TableName { get; }

    protected BaseRepository(DapperContext context)
    {
        _context = context;
    }

    public virtual async Task<T?> GetByIdAsync(Guid id)
    {
        var query = $"SELECT * FROM {TableName} WHERE Id = @Id AND IsDeleted = 0";
        
        using var connection = _context.CreateConnection();
        return await connection.QueryFirstOrDefaultAsync<T>(query, new { Id = id });
    }

    public virtual async Task<IEnumerable<T>> GetAllAsync()
    {
        var query = $"SELECT * FROM {TableName} WHERE IsDeleted = 0";
        
        using var connection = _context.CreateConnection();
        return await connection.QueryAsync<T>(query);
    }

    public virtual async Task<T> AddAsync(T entity)
    {
        entity.CreatedAt = DateTime.UtcNow;
        
        var columns = GetColumns(excludeKey: true);
        var stringOfColumns = string.Join(", ", columns);
        var stringOfParameters = string.Join(", ", columns.Select(e => "@" + e));
        
        var query = $"INSERT INTO {TableName} ({stringOfColumns}) VALUES ({stringOfParameters}); " +
                   $"SELECT CAST(SCOPE_IDENTITY() as uniqueidentifier)";

        using var connection = _context.CreateConnection();
        await connection.ExecuteAsync(query, entity);
        
        return entity;
    }

    public virtual async Task<bool> UpdateAsync(T entity)
    {
        entity.UpdatedAt = DateTime.UtcNow;
        
        var columns = GetColumns(excludeKey: true);
        var stringOfColumns = string.Join(", ", columns.Select(e => $"{e} = @{e}"));
        
        var query = $"UPDATE {TableName} SET {stringOfColumns} WHERE Id = @Id";

        using var connection = _context.CreateConnection();
        var affectedRows = await connection.ExecuteAsync(query, entity);
        
        return affectedRows > 0;
    }

    public virtual async Task<bool> DeleteAsync(Guid id)
    {
        var query = $"UPDATE {TableName} SET IsDeleted = 1, UpdatedAt = @UpdatedAt WHERE Id = @Id";

        using var connection = _context.CreateConnection();
        var affectedRows = await connection.ExecuteAsync(query, new { Id = id, UpdatedAt = DateTime.UtcNow });
        
        return affectedRows > 0;
    }

    public virtual async Task<bool> ExistsAsync(Guid id)
    {
        var query = $"SELECT COUNT(1) FROM {TableName} WHERE Id = @Id AND IsDeleted = 0";
        
        using var connection = _context.CreateConnection();
        var count = await connection.ExecuteScalarAsync<int>(query, new { Id = id });
        
        return count > 0;
    }

    protected virtual IEnumerable<string> GetColumns(bool excludeKey = false)
    {
        var type = typeof(T);
        var columns = type.GetProperties()
            .Where(p => !excludeKey || p.Name != "Id")
            .Select(p => p.Name);

        return columns;
    }
}
