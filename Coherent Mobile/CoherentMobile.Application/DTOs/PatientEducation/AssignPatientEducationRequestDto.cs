namespace CoherentMobile.Application.DTOs.PatientEducation;

public class AssignPatientEducationRequestDto
{
    public int PatientId { get; set; }
    public int EducationId { get; set; }
    public string? Notes { get; set; }
    public string? ArNotes { get; set; }
    public DateTime? ExpiresAt { get; set; }
}
