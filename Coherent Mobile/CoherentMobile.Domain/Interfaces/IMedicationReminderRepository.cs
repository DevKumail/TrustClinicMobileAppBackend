
using CoherentMobile.Domain.Entities;

namespace CoherentMobile.Domain.Interfaces;

public interface IMedicationReminderRepository
{
    Task<int> CreateAsync(MedicationReminder reminder);
    Task<int> UpdateAsync(MedicationReminder reminder);
    Task<MedicationReminder?> GetByIdAsync(int medicationReminderId);
    Task<IReadOnlyList<MedicationReminder>> GetUserAsync(int userId, string userType, bool activeOnly = true);
    Task<IReadOnlyList<MedicationReminder>> GetDueAsync(DateTime nowUtc, int take = 200);
    Task<int> MarkTriggeredAsync(int medicationReminderId, DateTime lastTriggeredAtUtc, DateTime? nextTriggerAtUtc, bool isActive);
    Task<int> DeactivateAsync(int medicationReminderId, int userId, string userType);
}

