using CoherentMobile.Domain.Entities;
using CoherentMobile.Domain.Interfaces;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace CoherentMobile.Infrastructure.Repositories;

/// <summary>
/// Repository for Doctor operations
/// </summary>
public class DoctorRepository : IDoctorRepository
{
    private readonly string _connectionString;

    public DoctorRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection") 
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found");
    }

    public async Task<IEnumerable<Doctor>> GetByFacilityIdAsync(int facilityId)
    {
        using var connection = new SqlConnection(_connectionString);
        
        const string query = @"
            SELECT d.DId, d.DoctorName, d.ArDoctorName, d.Title, d.ArTitle, d.SPId,
                   d.YearsOfExperience, d.Nationality, d.ArNationality, d.Languages, d.ArLanguages,
                   d.DoctorPhotoName, d.About, d.ArAbout, d.Education, d.ArEducation,
                   d.Experience, d.ArExperience, d.Expertise, d.ArExpertise,
                   d.LicenceNo, d.Active, d.Gender
            FROM MDoctors d
            INNER JOIN MDoctorFacilities df ON d.DId = df.DId
            WHERE df.FId = @FacilityId AND d.Active = 1
            ORDER BY d.DoctorName";

        return await connection.QueryAsync<Doctor>(query, new { FacilityId = facilityId });
    }

    public async Task<Doctor?> GetByIdAsync(int doctorId)
    {
        using var connection = new SqlConnection(_connectionString);
        
        const string query = @"
            SELECT DId, DoctorName, ArDoctorName, Title, ArTitle, SPId,
                   YearsOfExperience, Nationality, ArNationality, Languages, ArLanguages,
                   DoctorPhotoName, About, ArAbout, Education, ArEducation,
                   Experience, ArExperience, Expertise, ArExpertise,
                   LicenceNo, Active, Gender
            FROM MDoctors
            WHERE DId = @DoctorId";

        return await connection.QueryFirstOrDefaultAsync<Doctor>(query, new { DoctorId = doctorId });
    }
}
