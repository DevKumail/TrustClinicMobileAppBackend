namespace CoherentMobile.Application.DTOs.Clinic;

/// <summary>
/// Service Q&A DTO
/// </summary>
public class ServiceQnaDto
{
    public int Id { get; set; }
    public string Question { get; set; } = string.Empty;
    public string Answer { get; set; } = string.Empty;
}
