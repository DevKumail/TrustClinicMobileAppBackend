namespace CoherentMobile.Domain.Entities;

/// <summary>
/// Promotion entity for mobile app promotions/banners
/// </summary>
public class Promotion
{
    public int PromotionId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? ArTitle { get; set; }
    public string? Description { get; set; }
    public string? ArDescription { get; set; }
    public string ImageFileName { get; set; } = string.Empty;
    public string? LinkUrl { get; set; }
    public string? LinkType { get; set; }
    public int DisplayOrder { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public bool IsActive { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime CreatedAt { get; set; }
    public int? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public int? UpdatedBy { get; set; }
}
