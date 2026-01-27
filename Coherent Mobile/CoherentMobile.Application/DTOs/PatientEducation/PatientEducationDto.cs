namespace CoherentMobile.Application.DTOs.PatientEducation;

public class PatientEducationDto
{
    public int EducationId { get; set; }
    public int CategoryId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? ArTitle { get; set; }
    public string? PdfFileName { get; set; }
    public string? PdfFilePath { get; set; }
    public string? Summary { get; set; }
    public string? ArSummary { get; set; }
    public string? ThumbnailImageName { get; set; }
    public int? DisplayOrder { get; set; }
    public string? PdfUrl { get; set; }
    public string? ThumbnailUrl { get; set; }
    public bool Active { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public int? CreatedBy { get; set; }
    public int? UpdatedBy { get; set; }
    public string? ContentDeltaJson { get; set; }
    public string? ArContentDeltaJson { get; set; }
}
