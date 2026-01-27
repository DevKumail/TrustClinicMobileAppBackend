namespace CoherentMobile.Application.DTOs.PatientEducation;

public class PatientEducationAssignmentDto
{
    public int AssignmentId { get; set; }
    public int PatientId { get; set; }
    public int EducationId { get; set; }
    public DateTime AssignedAt { get; set; }
    public string? Notes { get; set; }
    public string? ArNotes { get; set; }
    public bool IsViewed { get; set; }
    public DateTime? ViewedAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public PatientEducationDto? Education { get; set; }
}
