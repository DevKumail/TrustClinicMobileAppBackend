namespace CoherentMobile.Application.DTOs;

public class DeviceTokenUpsertRequestDto
{
    public string Token { get; set; } = string.Empty;
    public string? Platform { get; set; }
}

public class DeviceTokenRemoveRequestDto
{
    public string Token { get; set; } = string.Empty;
}
