using CoherentMobile.Domain.Entities;

namespace CoherentMobile.Domain.Interfaces;

/// <summary>
/// QR Code Scan repository interface
/// </summary>
public interface IQRCodeScanRepository
{
    Task<QRCodeScan?> GetByIdAsync(int id);
    Task<QRCodeScan?> GetByQRCodeIdAsync(string qrCodeId);
    Task<IEnumerable<QRCodeScan>> GetByMRNOAsync(string mrno);
    Task<int> AddAsync(QRCodeScan scan);
    Task<bool> MarkAsCompletedAsync(int id);
    Task<IEnumerable<QRCodeScan>> GetPendingScansAsync();
}
