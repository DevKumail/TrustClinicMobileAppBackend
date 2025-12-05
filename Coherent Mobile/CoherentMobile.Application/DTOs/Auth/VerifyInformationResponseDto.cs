namespace CoherentMobile.Application.DTOs.Auth;

/// <summary>
/// Response after verifying user information
/// </summary>
public class VerifyInformationResponseDto
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public string DeliveryChannel { get; set; } = string.Empty; // "SMS" or "Email"
    public int ExpiresIn { get; set; } // seconds (default 300 = 5 minutes)
    public string? RecipientContact { get; set; } // masked mobile/email
}
