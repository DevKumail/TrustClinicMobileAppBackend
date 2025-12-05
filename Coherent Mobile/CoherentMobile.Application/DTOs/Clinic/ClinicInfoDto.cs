namespace CoherentMobile.Application.DTOs.Clinic;

/// <summary>
/// Complete clinic information DTO for guest mode
/// </summary>
public class ClinicInfoDto
{
    public int Id { get; set; }
    public List<string> ClinicsImages { get; set; } = new();
    public List<string> Locations { get; set; } = new();
    public string Description { get; set; } = string.Empty;
    public List<DoctorDto> Doctors { get; set; } = new();
    public List<ServiceDto> Services { get; set; } = new();
}
