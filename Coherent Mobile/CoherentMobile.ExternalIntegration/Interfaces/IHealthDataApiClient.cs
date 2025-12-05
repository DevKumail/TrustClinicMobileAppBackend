using CoherentMobile.ExternalIntegration.Models;

namespace CoherentMobile.ExternalIntegration.Interfaces;

/// <summary>
/// Interface for external health data API integration
/// </summary>
public interface IHealthDataApiClient
{
    Task<ExternalHealthDataResponse?> GetHealthDataAsync(string userId, string dataType);
    Task<bool> SyncHealthDataAsync(string userId, HealthData healthData);
    Task<IEnumerable<HealthData>> GetHistoricalDataAsync(string userId, DateTime startDate, DateTime endDate);
}
