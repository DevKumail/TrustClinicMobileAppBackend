namespace CoherentMobile.ExternalIntegration.Models;

public class Diagnosis
{
    public int Id { get; set; }
    public int VisitAccountNo { get; set; }
    public string IcD9Code { get; set; } = string.Empty;
    public bool Confidential { get; set; }
    public string LastUpdatedBy { get; set; } = string.Empty;
    public string LastUpdatedDate { get; set; } = string.Empty;
    public string Mrno { get; set; } = string.Empty;
    public string IcD9Description { get; set; } = string.Empty;
    public int ProviderId { get; set; }
    public string VisitDate { get; set; } = string.Empty;
    public string DoctorName { get; set; } = string.Empty;
    public string? Speciality { get; set; }
}
