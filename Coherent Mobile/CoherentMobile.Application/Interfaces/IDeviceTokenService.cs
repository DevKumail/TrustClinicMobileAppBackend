using CoherentMobile.Application.DTOs;

namespace CoherentMobile.Application.Interfaces;

public interface IDeviceTokenService
{
    Task UpsertAsync(int userId, string userType, DeviceTokenUpsertRequestDto request);
    Task RemoveAsync(int userId, string userType, DeviceTokenRemoveRequestDto request);
}
