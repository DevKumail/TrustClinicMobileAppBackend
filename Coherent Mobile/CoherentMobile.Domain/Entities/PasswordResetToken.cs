namespace CoherentMobile.Domain.Entities;

/// <summary>
/// Password reset token entity
/// </summary>
public class PasswordResetToken
{
    public int Id { get; set; }
    
    public int PatientId { get; set; }
    public string Token { get; set; } = string.Empty;
    
    // Token Status
    public bool IsUsed { get; set; }
    public bool IsExpired { get; set; }
    
    // Timing
    public DateTime ExpiresAt { get; set; }
    public DateTime? UsedAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation Property
    public Patient Patient { get; set; } = null!;
}
