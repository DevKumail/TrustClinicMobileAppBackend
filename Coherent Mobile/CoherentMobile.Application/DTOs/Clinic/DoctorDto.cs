namespace CoherentMobile.Application.DTOs.Clinic;

/// <summary>
/// Doctor information DTO for guest mode
/// </summary>
public class DoctorDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Position { get; set; } = string.Empty;
    public string Speciality { get; set; } = string.Empty;
    public string ProvNPI { get; set; } = string.Empty;
    public string Experience { get; set; } = string.Empty;
    public string Nationality { get; set; } = string.Empty;
    public List<string> Languages { get; set; } = new();
    public List<string> Location { get; set; } = new();
    public string ImageUrl { get; set; } = string.Empty;
    public string Biography { get; set; } = string.Empty;
}
