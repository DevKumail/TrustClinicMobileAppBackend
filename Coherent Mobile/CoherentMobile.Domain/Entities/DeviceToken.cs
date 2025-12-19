namespace CoherentMobile.Domain.Entities;

public class DeviceToken
{
    public int DeviceTokenId { get; set; }
    public int UserId { get; set; }
    public string UserType { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;
    public string? Platform { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
