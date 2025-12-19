using CoherentMobile.Domain.Entities;

namespace CoherentMobile.Domain.Interfaces;

public interface IDeviceTokenRepository
{
    Task UpsertAsync(int userId, string userType, string token, string? platform);
    Task<IReadOnlyList<DeviceToken>> GetActiveAsync(int userId, string userType);
    Task DeactivateAsync(int userId, string userType, string token);
}
