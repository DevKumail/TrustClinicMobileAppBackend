namespace CoherentMobile.ExternalIntegration.Models;

public class Medication
{
    public int MedicationId { get; set; }
    public string Mrno { get; set; } = string.Empty;
    public int VisitAccountNo { get; set; }
    public string Rx { get; set; } = string.Empty;
    public string Dose { get; set; } = string.Empty;
    public string ProviderName { get; set; } = string.Empty;
    public string Route { get; set; } = string.Empty;
    public string Frequency { get; set; } = string.Empty;
    public string Duration { get; set; } = string.Empty;
    public string Quantity { get; set; } = string.Empty;
    public string PrescriptionDate { get; set; } = string.Empty;
    public string StartDate { get; set; } = string.Empty;
    public string StopDate { get; set; } = string.Empty;
    public string DaysLeft { get; set; } = string.Empty;
    public string ProviderImage { get; set; } = string.Empty;
    public string Instructions { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
}
