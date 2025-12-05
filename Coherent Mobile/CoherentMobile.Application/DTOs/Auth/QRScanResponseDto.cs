namespace CoherentMobile.Application.DTOs.Auth;

/// <summary>
/// Response from QR Code scan (comes from external web portal)
/// </summary>
public class QRScanResponseDto
{
    public string MRNO { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
    public string EmiratesIdType { get; set; } = string.Empty; // "Emirates" or "Passport"
}
