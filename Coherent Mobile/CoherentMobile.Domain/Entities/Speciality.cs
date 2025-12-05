namespace CoherentMobile.Domain.Entities;

/// <summary>
/// Speciality entity - Maps to MSpecility table
/// </summary>
public class Speciality
{
    public int SPId { get; set; }
    public string? SpecilityName { get; set; }
    public string? ArSpecilityName { get; set; }
    public bool? Active { get; set; }
    public int? FId { get; set; }
}
