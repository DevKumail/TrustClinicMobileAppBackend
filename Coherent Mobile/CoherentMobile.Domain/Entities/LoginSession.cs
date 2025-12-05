namespace CoherentMobile.Domain.Entities;

/// <summary>
/// Login session/token tracking entity
/// </summary>
public class LoginSession
{
    public int Id { get; set; }
    
    public int PatientId { get; set; }
    
    // Token Details
    public string AccessToken { get; set; } = string.Empty;
    public string? RefreshToken { get; set; }
    
    // Session Info
    public string? IPAddress { get; set; }
    public string? DeviceInfo { get; set; }
    public string? UserAgent { get; set; }
    
    // Session Status
    public bool IsActive { get; set; } = true;
    public bool IsRevoked { get; set; }
    
    // Timing
    public DateTime ExpiresAt { get; set; }
    public DateTime LastActivityAt { get; set; } = DateTime.UtcNow;
    public DateTime? RevokedAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation Property
    public Patient Patient { get; set; } = null!;
}
