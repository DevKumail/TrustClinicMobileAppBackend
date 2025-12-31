namespace CoherentMobile.Application.DTOs;

public class NotificationDto
{
    public int NotificationId { get; set; }
    public string NotificationType { get; set; } = string.Empty;
    public string? Title { get; set; }
    public string? Body { get; set; }
    public string? DataJson { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? DeliveredAt { get; set; }
    public DateTime? ReadAt { get; set; }
}

public class NotificationsGetResponseDto
{
    public DateTime ServerTimeUtc { get; set; }
    public List<NotificationDto> Notifications { get; set; } = new();
}

public class NotificationsAckRequestDto
{
    public List<int> NotificationIds { get; set; } = new();
    public bool MarkRead { get; set; }
}

public class NotificationsAckResponseDto
{
    public int AffectedCount { get; set; }
}

public class NotificationCreateRequestDto
{
    public int UserId { get; set; }
    public string UserType { get; set; } = "Patient";
    public string NotificationType { get; set; } = string.Empty;
    public string? Title { get; set; }
    public string? Body { get; set; }
    public string? DataJson { get; set; }
}
