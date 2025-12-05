namespace CoherentMobile.Application.DTOs.Auth;

/// <summary>
/// Combined request DTO for verify information with QR data
/// </summary>
public class VerifyInformationWithQRRequestDto
{
    /// <summary>
    /// QR code data scanned from web portal
    /// </summary>
    public QRScanResponseDto? QRData { get; set; }
    
    /// <summary>
    /// User verification information
    /// </summary>
    public VerifyInformationRequestDto VerificationInfo { get; set; } = new();
}
