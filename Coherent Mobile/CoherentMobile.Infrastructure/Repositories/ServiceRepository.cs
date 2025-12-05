using CoherentMobile.Domain.Entities;
using CoherentMobile.Domain.Interfaces;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace CoherentMobile.Infrastructure.Repositories;

/// <summary>
/// Repository for Service operations
/// </summary>
public class ServiceRepository : IServiceRepository
{
    private readonly string _connectionString;

    public ServiceRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection") 
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found");
    }

    public async Task<IEnumerable<Service>> GetByFacilityIdAsync(int facilityId)
    {
        using var connection = new SqlConnection(_connectionString);
        
        const string query = @"
            SELECT SId, FId, ServiceTitle, ArServiceTitle, ServiceIntro, ArServiceIntro,
                   Active, DisplayOrder, DisplayImageName, IconImageName
            FROM MServices
            WHERE FId = @FacilityId AND Active = 1
            ORDER BY DisplayOrder";

        return await connection.QueryAsync<Service>(query, new { FacilityId = facilityId });
    }

    public async Task<IEnumerable<SubService>> GetSubServicesByServiceIdAsync(int serviceId)
    {
        using var connection = new SqlConnection(_connectionString);
        
        const string query = @"
            SELECT SSId, SubServiceTitle, ArSubServiceTitle, Details, ArDetails,
                   DisplayOrder, Active, FId, SId
            FROM MSubServices
            WHERE SId = @ServiceId AND Active = 1
            ORDER BY DisplayOrder";

        return await connection.QueryAsync<SubService>(query, new { ServiceId = serviceId });
    }
}
