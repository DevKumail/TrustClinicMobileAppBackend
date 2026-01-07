namespace CoherentMobile.Application.DTOs.Clinic;

/// <summary>
/// Service Detail DTO
/// </summary>
public class ServiceDetailDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Details { get; set; } = string.Empty;
}
