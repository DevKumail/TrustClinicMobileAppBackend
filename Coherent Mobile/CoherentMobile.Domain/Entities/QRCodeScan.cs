namespace CoherentMobile.Domain.Entities;

/// <summary>
/// QR Code scan audit trail
/// </summary>
public class QRCodeScan
{
    public int Id { get; set; }
    
    public string QRCodeId { get; set; } = string.Empty;
    public string MRNO { get; set; } = string.Empty;
    
    // Scan Details
    public DateTime ScannedAt { get; set; } = DateTime.UtcNow;
    public string? IPAddress { get; set; }
    public string? DeviceInfo { get; set; }
    
    // Status
    public bool IsSignupCompleted { get; set; }
    public DateTime? CompletedAt { get; set; }
}
