using CoherentMobile.Domain.Entities;
using CoherentMobile.Domain.Interfaces;
using CoherentMobile.Infrastructure.Data;
using Dapper;

namespace CoherentMobile.Infrastructure.Repositories;

/// <summary>
/// Promotion repository implementation using Dapper
/// </summary>
public class PromotionRepository : IPromotionRepository
{
    private readonly DapperContext _context;
    private const string TableName = "MPromotion";

    public PromotionRepository(DapperContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Promotion>> GetAllActiveAsync()
    {
        var query = $@"SELECT * FROM {TableName} 
                       WHERE IsDeleted = 0 AND IsActive = 1 
                       ORDER BY DisplayOrder";
        
        using var connection = _context.CreateConnection();
        return await connection.QueryAsync<Promotion>(query);
    }
}
