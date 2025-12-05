using System.Data;
using System.Reflection;
using CoherentMobile.Application.Exceptions;
using CoherentMobile.Infrastructure.Data;
using Dapper;
using Microsoft.Data.SqlClient;

namespace CoherentMobile.Infrastructure.Repositories;

/// <summary>
/// Generic Dapper repository for all CRUD operations with int ID
/// </summary>
/// <typeparam name="T">Entity type</typeparam>
public abstract class GenericRepository<T> where T : class
{
    protected readonly DapperContext _context;
    protected abstract string TableName { get; }

    protected GenericRepository(DapperContext context)
    {
        _context = context;
    }

    public virtual async Task<T?> GetByIdAsync(int id)
    {
        try
        {
            var query = $"SELECT * FROM {TableName} WHERE Id = @Id";
            
            using var connection = _context.CreateConnection();
            return await connection.QueryFirstOrDefaultAsync<T>(query, new { Id = id });
        }
        catch (SqlException ex)
        {
            throw new DatabaseException($"Failed to get {typeof(T).Name} by ID: {id}", ex, "DB_GET_BY_ID_ERROR");
        }
    }

    public virtual async Task<IEnumerable<T>> GetAllAsync()
    {
        try
        {
            var query = $"SELECT * FROM {TableName}";
            
            using var connection = _context.CreateConnection();
            return await connection.QueryAsync<T>(query);
        }
        catch (SqlException ex)
        {
            throw new DatabaseException($"Failed to get all {typeof(T).Name}", ex, "DB_GET_ALL_ERROR");
        }
    }

    public virtual async Task<int> AddAsync(T entity)
    {
        try
        {
            var columns = GetColumns(excludeKey: true);
            var stringOfColumns = string.Join(", ", columns);
            var stringOfParameters = string.Join(", ", columns.Select(e => "@" + e));
            
            var query = $"INSERT INTO {TableName} ({stringOfColumns}) VALUES ({stringOfParameters}); " +
                       $"SELECT CAST(SCOPE_IDENTITY() as int)";

            using var connection = _context.CreateConnection();
            var id = await connection.QuerySingleAsync<int>(query, entity);
            
            return id;
        }
        catch (SqlException ex) when (ex.Number == 2627 || ex.Number == 2601) // UNIQUE constraint violation
        {
            var errorMessage = $"Duplicate entry detected when adding {typeof(T).Name}. ";
            if (ex.Message.Contains("EmiratesId"))
                errorMessage += "Emirates ID already exists.";
            else if (ex.Message.Contains("PassportNumber"))
                errorMessage += "Passport number already exists.";
            else if (ex.Message.Contains("Email"))
                errorMessage += "Email already exists.";
            else if (ex.Message.Contains("MobileNumber"))
                errorMessage += "Mobile number already exists.";
            else if (ex.Message.Contains("MRNO"))
                errorMessage += "MRNO already exists.";
            else
                errorMessage += ex.Message;
                
            throw new DatabaseException(errorMessage, ex, "DB_DUPLICATE_ERROR");
        }
        catch (SqlException ex)
        {
            throw new DatabaseException($"Failed to add {typeof(T).Name}: {ex.Message}", ex, "DB_ADD_ERROR");
        }
    }

    public virtual async Task<bool> UpdateAsync(T entity)
    {
        try
        {
            var columns = GetColumns(excludeKey: true);
            var stringOfColumns = string.Join(", ", columns.Select(e => $"{e} = @{e}"));
            
            var query = $"UPDATE {TableName} SET {stringOfColumns} WHERE Id = @Id";

            using var connection = _context.CreateConnection();
            var affectedRows = await connection.ExecuteAsync(query, entity);
            
            return affectedRows > 0;
        }
        catch (SqlException ex)
        {
            throw new DatabaseException($"Failed to update {typeof(T).Name}", ex, "DB_UPDATE_ERROR");
        }
    }

    public virtual async Task<bool> DeleteAsync(int id)
    {
        try
        {
            var query = $"DELETE FROM {TableName} WHERE Id = @Id";

            using var connection = _context.CreateConnection();
            var affectedRows = await connection.ExecuteAsync(query, new { Id = id });
            
            return affectedRows > 0;
        }
        catch (SqlException ex)
        {
            throw new DatabaseException($"Failed to delete {typeof(T).Name} with ID: {id}", ex, "DB_DELETE_ERROR");
        }
    }

    public virtual async Task<bool> ExistsAsync(int id)
    {
        try
        {
            var query = $"SELECT COUNT(1) FROM {TableName} WHERE Id = @Id";
            
            using var connection = _context.CreateConnection();
            var count = await connection.ExecuteScalarAsync<int>(query, new { Id = id });
            
            return count > 0;
        }
        catch (SqlException ex)
        {
            throw new DatabaseException($"Failed to check if {typeof(T).Name} exists with ID: {id}", ex, "DB_EXISTS_ERROR");
        }
    }

    /// <summary>
    /// Execute stored procedure and return single result
    /// </summary>
    protected async Task<T?> ExecuteStoredProcSingleAsync(string procedureName, object parameters)
    {
        try
        {
            using var connection = _context.CreateConnection();
            return await connection.QueryFirstOrDefaultAsync<T>(
                procedureName, 
                parameters, 
                commandType: CommandType.StoredProcedure
            );
        }
        catch (SqlException ex)
        {
            throw new DatabaseException($"Failed to execute stored procedure: {procedureName}", ex, "DB_SP_ERROR");
        }
    }

    /// <summary>
    /// Execute stored procedure and return multiple results
    /// </summary>
    protected async Task<IEnumerable<T>> ExecuteStoredProcAsync(string procedureName, object parameters)
    {
        try
        {
            using var connection = _context.CreateConnection();
            return await connection.QueryAsync<T>(
                procedureName, 
                parameters, 
                commandType: CommandType.StoredProcedure
            );
        }
        catch (SqlException ex)
        {
            throw new DatabaseException($"Failed to execute stored procedure: {procedureName}", ex, "DB_SP_ERROR");
        }
    }

    /// <summary>
    /// Execute stored procedure with no return
    /// </summary>
    protected async Task<int> ExecuteStoredProcNonQueryAsync(string procedureName, object parameters)
    {
        try
        {
            using var connection = _context.CreateConnection();
            return await connection.ExecuteAsync(
                procedureName, 
                parameters, 
                commandType: CommandType.StoredProcedure
            );
        }
        catch (SqlException ex)
        {
            throw new DatabaseException($"Failed to execute stored procedure: {procedureName}", ex, "DB_SP_ERROR");
        }
    }

    protected virtual IEnumerable<string> GetColumns(bool excludeKey = false)
    {
        var type = typeof(T);
        var columns = type.GetProperties()
            .Where(p => !excludeKey || p.Name != "Id")
            .Where(p => p.PropertyType.IsValueType || 
                       p.PropertyType == typeof(string) ||
                       Nullable.GetUnderlyingType(p.PropertyType) != null) // Include nullable types
            .Where(p => !typeof(System.Collections.IEnumerable).IsAssignableFrom(p.PropertyType) || 
                       p.PropertyType == typeof(string)) // Exclude collections except strings
            .Select(p => p.Name);

        return columns;
    }
}
