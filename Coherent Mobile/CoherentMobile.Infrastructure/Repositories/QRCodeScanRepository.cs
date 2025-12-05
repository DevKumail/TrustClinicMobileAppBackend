using CoherentMobile.Domain.Entities;
using CoherentMobile.Domain.Interfaces;
using CoherentMobile.Infrastructure.Data;
using Dapper;

namespace CoherentMobile.Infrastructure.Repositories;

/// <summary>
/// QR Code Scan repository implementation
/// </summary>
public class QRCodeScanRepository : GenericRepository<QRCodeScan>, IQRCodeScanRepository
{
    protected override string TableName => "QRCodeScans";

    public QRCodeScanRepository(DapperContext context) : base(context)
    {
    }

    public async Task<QRCodeScan?> GetByQRCodeIdAsync(string qrCodeId)
    {
        var query = "SELECT TOP 1 * FROM QRCodeScans WHERE QRCodeId = @QRCodeId ORDER BY ScannedAt DESC";
        
        using var connection = _context.CreateConnection();
        return await connection.QueryFirstOrDefaultAsync<QRCodeScan>(query, new { QRCodeId = qrCodeId });
    }

    public async Task<IEnumerable<QRCodeScan>> GetByMRNOAsync(string mrno)
    {
        var query = "SELECT * FROM QRCodeScans WHERE MRNO = @MRNO ORDER BY ScannedAt DESC";
        
        using var connection = _context.CreateConnection();
        return await connection.QueryAsync<QRCodeScan>(query, new { MRNO = mrno });
    }

    public async Task<bool> MarkAsCompletedAsync(int id)
    {
        var query = "UPDATE QRCodeScans SET IsSignupCompleted = 1, CompletedAt = @CompletedAt WHERE Id = @Id";
        
        using var connection = _context.CreateConnection();
        var affected = await connection.ExecuteAsync(query, new { Id = id, CompletedAt = DateTime.UtcNow });
        
        return affected > 0;
    }

    public async Task<IEnumerable<QRCodeScan>> GetPendingScansAsync()
    {
        var query = "SELECT * FROM QRCodeScans WHERE IsSignupCompleted = 0 ORDER BY ScannedAt DESC";
        
        using var connection = _context.CreateConnection();
        return await connection.QueryAsync<QRCodeScan>(query);
    }
}
