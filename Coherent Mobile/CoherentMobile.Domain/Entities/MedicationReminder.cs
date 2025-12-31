
namespace CoherentMobile.Domain.Entities;

public class MedicationReminder
{
    public int MedicationReminderId { get; set; }
    public int UserId { get; set; }
    public string UserType { get; set; } = string.Empty;
    public string? MedicationName { get; set; }
    public string? Dosage { get; set; }
    public string? Title { get; set; }
    public string? Body { get; set; }
    public string? DataJson { get; set; }
    public DateTime NextTriggerAtUtc { get; set; }
    public int? RepeatIntervalMinutes { get; set; }
    public DateTime? LastTriggeredAtUtc { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }
}

