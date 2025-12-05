using CoherentMobile.Domain.Entities;
using CoherentMobile.Domain.Interfaces;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace CoherentMobile.Infrastructure.Repositories;

/// <summary>
/// Repository for Speciality operations
/// </summary>
public class SpecialityRepository : ISpecialityRepository
{
    private readonly string _connectionString;

    public SpecialityRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection") 
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found");
    }

    public async Task<Speciality?> GetByIdAsync(int specialityId)
    {
        using var connection = new SqlConnection(_connectionString);
        
        const string query = @"
            SELECT SPId, SpecilityName, ArSpecilityName, Active, FId
            FROM MSpecility
            WHERE SPId = @SpecialityId";

        return await connection.QueryFirstOrDefaultAsync<Speciality>(query, new { SpecialityId = specialityId });
    }
}
