namespace CoherentMobile.Domain.Entities;

/// <summary>
/// Service entity - Maps to MServices table
/// </summary>
public class Service
{
    public int SId { get; set; }
    public int? FId { get; set; }
    public string? ServiceTitle { get; set; }
    public string? ArServiceTitle { get; set; }
    public string? ServiceIntro { get; set; }
    public string? ArServiceIntro { get; set; }
    public bool? Active { get; set; }
    public int? DisplayOrder { get; set; }
    public string? DisplayImageName { get; set; }
    public string? IconImageName { get; set; }
}
