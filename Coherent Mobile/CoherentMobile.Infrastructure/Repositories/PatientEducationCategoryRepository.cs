using CoherentMobile.Domain.Entities;
using CoherentMobile.Domain.Interfaces;
using CoherentMobile.Infrastructure.Data;
using Dapper;

namespace CoherentMobile.Infrastructure.Repositories;

public class PatientEducationCategoryRepository : IPatientEducationCategoryRepository
{
    private readonly DapperContext _context;
    private const string TableName = "MPatientEducationCategory";

    public PatientEducationCategoryRepository(DapperContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<PatientEducationCategory>> GetActiveAsync(bool? generalOnly)
    {
        var query = $@"
SELECT *
FROM {TableName}
WHERE IsDeleted = 0
  AND Active = 1
  AND (@GeneralOnly IS NULL OR IsGeneral = @GeneralOnly)
ORDER BY ISNULL(DisplayOrder, 999999), CategoryName";

        using var connection = _context.CreateConnection();
        return await connection.QueryAsync<PatientEducationCategory>(query, new { GeneralOnly = generalOnly });
    }

    public async Task<PatientEducationCategory?> GetByIdAsync(int categoryId)
    {
        var query = $@"
SELECT *
FROM {TableName}
WHERE CategoryId = @CategoryId
  AND IsDeleted = 0";

        using var connection = _context.CreateConnection();
        return await connection.QueryFirstOrDefaultAsync<PatientEducationCategory>(query, new { CategoryId = categoryId });
    }
}
