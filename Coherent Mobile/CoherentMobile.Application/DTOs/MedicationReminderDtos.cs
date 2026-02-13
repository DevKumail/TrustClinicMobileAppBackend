
namespace CoherentMobile.Application.DTOs;

public class MedicationReminderDto
{
    public int MedicationReminderId { get; set; }
    public string? MedicationName { get; set; }
    public string? Dosage { get; set; }
    public string? Title { get; set; }
    public string? Body { get; set; }
    public string? DataJson { get; set; }
    public DateTime NextTriggerAtUtc { get; set; }
    public int? RepeatIntervalMinutes { get; set; }
    public DateTime? LastTriggeredAtUtc { get; set; }
    public bool IsActive { get; set; }
}

public class MedicationReminderUpsertRequestDto
{
    public string? MedicationName { get; set; }
    public string? Dosage { get; set; }
    public string? Title { get; set; }
    public string? Body { get; set; }
    public string? DataJson { get; set; }
    public DateTime NextTriggerAtUtc { get; set; }
    public int? RepeatIntervalMinutes { get; set; }
}

public class MedicationReminderActionRequestDto
{
    public int? ActionId { get; set; }
    public string Action { get; set; } = string.Empty;
}

