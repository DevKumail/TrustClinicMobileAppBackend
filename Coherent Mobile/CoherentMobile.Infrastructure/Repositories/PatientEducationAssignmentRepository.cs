using CoherentMobile.Domain.Entities;
using CoherentMobile.Domain.Interfaces;
using CoherentMobile.Infrastructure.Data;
using Dapper;

namespace CoherentMobile.Infrastructure.Repositories;

public class PatientEducationAssignmentRepository : IPatientEducationAssignmentRepository
{
    private readonly DapperContext _context;
    private const string TableName = "TPatientEducationAssignment";

    public PatientEducationAssignmentRepository(DapperContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<PatientEducationAssignment>> GetMyActiveAsync(int patientId, bool includeExpired)
    {
        var query = $@"
SELECT *
FROM {TableName}
WHERE PatientId = @PatientId
  AND IsDeleted = 0
  AND IsActive = 1
  AND (
        @IncludeExpired = 1
        OR ExpiresAt IS NULL
        OR ExpiresAt >= @NowUtc
      )
ORDER BY AssignedAt DESC";

        using var connection = _context.CreateConnection();
        return await connection.QueryAsync<PatientEducationAssignment>(query, new { PatientId = patientId, IncludeExpired = includeExpired ? 1 : 0, NowUtc = DateTime.UtcNow });
    }

    public async Task<PatientEducationAssignment?> GetByIdAsync(int assignmentId)
    {
        var query = $@"
SELECT *
FROM {TableName}
WHERE AssignmentId = @AssignmentId
  AND IsDeleted = 0";

        using var connection = _context.CreateConnection();
        return await connection.QueryFirstOrDefaultAsync<PatientEducationAssignment>(query, new { AssignmentId = assignmentId });
    }

    public async Task<int> AddAsync(PatientEducationAssignment assignment)
    {
        var query = $@"
INSERT INTO {TableName}
(
    PatientId,
    EducationId,
    AssignedByUserId,
    AssignedAt,
    Notes,
    ArNotes,
    IsViewed,
    ViewedAt,
    ExpiresAt,
    IsActive,
    IsDeleted,
    CreatedAt,
    UpdatedAt
)
VALUES
(
    @PatientId,
    @EducationId,
    @AssignedByUserId,
    @AssignedAt,
    @Notes,
    @ArNotes,
    @IsViewed,
    @ViewedAt,
    @ExpiresAt,
    @IsActive,
    @IsDeleted,
    @CreatedAt,
    @UpdatedAt
);
SELECT CAST(SCOPE_IDENTITY() as int);";

        using var connection = _context.CreateConnection();
        return await connection.QuerySingleAsync<int>(query, assignment);
    }

    public async Task<bool> MarkViewedAsync(int assignmentId, DateTime viewedAtUtc)
    {
        var query = $@"
UPDATE {TableName}
SET IsViewed = 1,
    ViewedAt = @ViewedAt,
    UpdatedAt = @UpdatedAt
WHERE AssignmentId = @AssignmentId
  AND IsDeleted = 0";

        using var connection = _context.CreateConnection();
        var affected = await connection.ExecuteAsync(query, new { AssignmentId = assignmentId, ViewedAt = viewedAtUtc, UpdatedAt = DateTime.UtcNow });
        return affected > 0;
    }
}
