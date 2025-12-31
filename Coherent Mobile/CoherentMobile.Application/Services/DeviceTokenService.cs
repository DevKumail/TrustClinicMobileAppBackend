using CoherentMobile.Application.DTOs;
using CoherentMobile.Application.Interfaces;
using CoherentMobile.Domain.Interfaces;

namespace CoherentMobile.Application.Services;

public class DeviceTokenService : IDeviceTokenService
{
    private readonly IDeviceTokenRepository _repository;

    public DeviceTokenService(IDeviceTokenRepository repository)
    {
        _repository = repository;
    }

    public Task UpsertAsync(int userId, string userType, DeviceTokenUpsertRequestDto request)
    {
        if (string.IsNullOrWhiteSpace(request.Token))
            throw new ArgumentException("Token is required", nameof(request));

        return _repository.UpsertAsync(userId, userType, request.Token.Trim(), request.Platform);
    }

    public Task RemoveAsync(int userId, string userType, DeviceTokenRemoveRequestDto request)
    {
        if (string.IsNullOrWhiteSpace(request.Token))
            throw new ArgumentException("Token is required", nameof(request));

        return _repository.DeactivateAsync(userId, userType, request.Token.Trim());
    }
}
