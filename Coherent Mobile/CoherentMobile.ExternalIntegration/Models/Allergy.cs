namespace CoherentMobile.ExternalIntegration.Models;

public class Allergy
{
    public int AllergyId { get; set; }
    public string Mrno { get; set; } = string.Empty;
    public int VisitAccountNo { get; set; }
    public int TypeId { get; set; }
    public string? AllergyType { get; set; }
    public string? ViewAllergyTypeName { get; set; }
    public string Allergen { get; set; } = string.Empty;
    public string Reaction { get; set; } = string.Empty;
    public string Severity { get; set; } = string.Empty;
    public string? OnsetDate { get; set; }
    public string? StartDate { get; set; }
    public string? EndDate { get; set; }
    public int Status { get; set; }
    public int ProviderId { get; set; }
    public string? CreatedDate { get; set; }
    public string? SeverityCode { get; set; }
    public string? Notes { get; set; }
    public bool IsActive { get; set; }
}
