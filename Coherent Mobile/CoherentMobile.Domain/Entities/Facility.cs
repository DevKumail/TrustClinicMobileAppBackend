namespace CoherentMobile.Domain.Entities;

/// <summary>
/// Facility (Clinic) entity - Maps to MFacility table
/// </summary>
public class Facility
{
    public int FId { get; set; }
    public string? FName { get; set; }
    public string? LicenceNo { get; set; }
    public string? AddressLine1 { get; set; }
    public string? AddressLine2 { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? Country { get; set; }
    public string? Phone1 { get; set; }
    public string? Phone2 { get; set; }
    public string? EmailAddress { get; set; }
    public string? WebsiteUrl { get; set; }
    public string? FbUrl { get; set; }
    public string? LinkedInUrl { get; set; }
    public string? YoutubeUrl { get; set; }
    public string? TwitterUrl { get; set; }
    public string? TiktokUrl { get; set; }
    public string? Instagram { get; set; }
    public string? WhatsappNo { get; set; }
    public string? GoogleMapUrl { get; set; }
    public string? About { get; set; }
    public string? AboutShort { get; set; }
    public string? ArAbout { get; set; }
    public string? ArAboutShort { get; set; }
    public string? FacilityImages { get; set; } // Comma-separated image file names
}
