using CoherentMobile.Domain.Entities;
using CoherentMobile.Domain.Interfaces;
using CoherentMobile.Infrastructure.Data;
using Dapper;

namespace CoherentMobile.Infrastructure.Repositories;

public class PatientEducationRepository : IPatientEducationRepository
{
    private readonly DapperContext _context;
    private const string TableName = "MPatientEducation";

    public PatientEducationRepository(DapperContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<PatientEducation>> GetActiveAsync(int? categoryId, string? search, int? limit)
    {
        var take = limit.HasValue && limit.Value > 0 ? limit.Value : 200;
        var q = (search ?? string.Empty).Trim();

        var query = $@"
SELECT TOP (@Take) *
FROM {TableName}
WHERE IsDeleted = 0
  AND Active = 1
  AND (@CategoryId IS NULL OR CategoryId = @CategoryId)
  AND (
        @Search = ''
        OR Title LIKE '%' + @Search + '%'
        OR ArTitle LIKE '%' + @Search + '%'
        OR Summary LIKE '%' + @Search + '%'
        OR ArSummary LIKE '%' + @Search + '%'
      )
ORDER BY ISNULL(DisplayOrder, 999999), Title";

        using var connection = _context.CreateConnection();
        return await connection.QueryAsync<PatientEducation>(query, new { CategoryId = categoryId, Search = q, Take = take });
    }

    public async Task<PatientEducation?> GetByIdAsync(int educationId)
    {
        var query = $@"
SELECT *
FROM {TableName}
WHERE EducationId = @EducationId
  AND IsDeleted = 0";

        using var connection = _context.CreateConnection();
        return await connection.QueryFirstOrDefaultAsync<PatientEducation>(query, new { EducationId = educationId });
    }
}
