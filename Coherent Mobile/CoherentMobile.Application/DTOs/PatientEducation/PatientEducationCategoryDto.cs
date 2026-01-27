namespace CoherentMobile.Application.DTOs.PatientEducation;

public class PatientEducationCategoryDto
{
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public string? ArCategoryName { get; set; }
    public string? CategoryDescription { get; set; }
    public string? ArCategoryDescription { get; set; }
    public string? IconImageName { get; set; }
    public int? DisplayOrder { get; set; }
    public bool IsGeneral { get; set; }
}
