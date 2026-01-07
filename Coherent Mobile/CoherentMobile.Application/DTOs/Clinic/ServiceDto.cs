namespace CoherentMobile.Application.DTOs.Clinic;

/// <summary>
/// Service information DTO
/// </summary>
public class ServiceDto
{
    public int Id { get; set; }
    public string ServiceIcon { get; set; } = string.Empty;
    public string ServiceName { get; set; } = string.Empty;
    public string ServiceImage { get; set; } = string.Empty;
    public List<ServiceDetailDto> ServiceDetails { get; set; } = new();
}
