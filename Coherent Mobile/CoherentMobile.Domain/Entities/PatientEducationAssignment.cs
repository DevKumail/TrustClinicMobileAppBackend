namespace CoherentMobile.Domain.Entities;

public class PatientEducationAssignment
{
    public int AssignmentId { get; set; }
    public int PatientId { get; set; }
    public int EducationId { get; set; }
    public int? AssignedByUserId { get; set; }
    public DateTime AssignedAt { get; set; }
    public string? Notes { get; set; }
    public string? ArNotes { get; set; }
    public bool IsViewed { get; set; }
    public DateTime? ViewedAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public bool IsActive { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
