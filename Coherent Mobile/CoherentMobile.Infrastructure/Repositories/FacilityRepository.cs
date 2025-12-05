using CoherentMobile.Domain.Entities;
using CoherentMobile.Domain.Interfaces;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace CoherentMobile.Infrastructure.Repositories;

/// <summary>
/// Repository for Facility (Clinic) operations
/// </summary>
public class FacilityRepository : IFacilityRepository
{
    private readonly string _connectionString;

    public FacilityRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection") 
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found");
    }

    public async Task<Facility?> GetByIdAsync(int facilityId)
    {
        using var connection = new SqlConnection(_connectionString);
        
        const string query = @"
            SELECT FId, FName, LicenceNo, AddressLine1, AddressLine2, City, State, Country,
                   Phone1, Phone2, EmailAddress, WebsiteUrl, FbUrl, LinkedInUrl, YoutubeUrl,
                   TwitterUrl, TiktokUrl, Instagram, WhatsappNo, GoogleMapUrl, About, AboutShort,
                   ArAbout, ArAboutShort, FacilityImages
            FROM MFacility
            WHERE FId = @FacilityId";

        return await connection.QueryFirstOrDefaultAsync<Facility>(query, new { FacilityId = facilityId });
    }

    public async Task<IEnumerable<Facility>> GetAllActiveAsync()
    {
        using var connection = new SqlConnection(_connectionString);
        
        const string query = @"
            SELECT FId, FName, LicenceNo, AddressLine1, AddressLine2, City, State, Country,
                   Phone1, Phone2, EmailAddress, WebsiteUrl, FbUrl, LinkedInUrl, YoutubeUrl,
                   TwitterUrl, TiktokUrl, Instagram, WhatsappNo, GoogleMapUrl, About, AboutShort,
                   ArAbout, ArAboutShort, FacilityImages
            FROM MFacility";

        return await connection.QueryAsync<Facility>(query);
    }
}
