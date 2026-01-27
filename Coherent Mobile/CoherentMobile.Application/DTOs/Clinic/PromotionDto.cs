namespace CoherentMobile.Application.DTOs.Clinic;

/// <summary>
/// Promotion DTO for mobile app banners/promotions
/// </summary>
public class PromotionDto
{
    public int PromotionId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? ArTitle { get; set; }
    public string? Description { get; set; }
    public string? ArDescription { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public string? LinkUrl { get; set; }
    public string? LinkType { get; set; }
    public int DisplayOrder { get; set; }
}
