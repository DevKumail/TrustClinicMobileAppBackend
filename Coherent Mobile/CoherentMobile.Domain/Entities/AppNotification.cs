namespace CoherentMobile.Domain.Entities;

public class AppNotification
{
    public int NotificationId { get; set; }
    public int UserId { get; set; }
    public string UserType { get; set; } = string.Empty;
    public string NotificationType { get; set; } = string.Empty;
    public string? Title { get; set; }
    public string? Body { get; set; }
    public string? DataJson { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? DeliveredAt { get; set; }
    public DateTime? ReadAt { get; set; }
    public bool IsDeleted { get; set; }
}
