
using CoherentMobile.Application.DTOs;

namespace CoherentMobile.Application.Interfaces;

public interface IMedicationReminderService
{
    Task<IReadOnlyList<MedicationReminderDto>> GetMyAsync(int userId, string userType, bool activeOnly = true);
    Task<int> CreateAsync(int userId, string userType, MedicationReminderUpsertRequestDto request);
    Task<bool> UpdateAsync(int userId, string userType, int medicationReminderId, MedicationReminderUpsertRequestDto request);
    Task<bool> DeactivateAsync(int userId, string userType, int medicationReminderId);
    Task<int> ProcessDueRemindersAsync(DateTime nowUtc, int take = 200);
}

