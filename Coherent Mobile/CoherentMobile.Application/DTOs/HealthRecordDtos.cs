namespace CoherentMobile.Application.DTOs;

/// <summary>
/// DTO for creating a health record
/// </summary>
public class CreateHealthRecordDto
{
    public string RecordType { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string Unit { get; set; } = string.Empty;
    public DateTime RecordedAt { get; set; }
    public string? Notes { get; set; }
}

/// <summary>
/// DTO for health record response
/// </summary>
public class HealthRecordDto
{
    public Guid Id { get; set; }
    public int UserId { get; set; }
    public string RecordType { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string Unit { get; set; } = string.Empty;
    public DateTime RecordedAt { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
}
