namespace CoherentMobile.Domain.Entities;

/// <summary>
/// Health record entity for storing user health data
/// </summary>
public class HealthRecord : BaseEntity
{
    public int UserId { get; set; }
    public string RecordType { get; set; } = string.Empty; // e.g., "BloodPressure", "HeartRate", "Weight"
    public string Value { get; set; } = string.Empty;
    public string Unit { get; set; } = string.Empty;
    public DateTime RecordedAt { get; set; }
    public string? Notes { get; set; }
    
    // Navigation property
    public Patient Patient { get; set; } = null!;
}
