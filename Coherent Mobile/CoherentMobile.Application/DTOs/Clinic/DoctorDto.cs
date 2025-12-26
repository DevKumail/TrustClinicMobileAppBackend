namespace CoherentMobile.Application.DTOs.Clinic;

/// <summary>
/// Doctor information DTO for guest mode
/// </summary>
public class DoctorDto
{
    public int DId { get; set; }
    public string? DoctorName { get; set; }
    public string? ArDoctorName { get; set; }
    public string? Title { get; set; }
    public string? ArTitle { get; set; }
    public int? SPId { get; set; }
    public string? YearsOfExperience { get; set; }
    public string? NationalityRaw { get; set; }
    public string? ArNationality { get; set; }
    public string? LanguagesRaw { get; set; }
    public string? ArLanguages { get; set; }
    public string? DoctorPhotoName { get; set; }
    public string? About { get; set; }
    public string? ArAbout { get; set; }
    public string? Education { get; set; }
    public string? ArEducation { get; set; }
    public string? ExperienceRaw { get; set; }
    public string? ArExperience { get; set; }
    public string? ExpertiseRaw { get; set; }
    public string? ArExpertise { get; set; }
    public string? LicenceNo { get; set; }
    public bool? Active { get; set; }
    public string? Gender { get; set; }

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
