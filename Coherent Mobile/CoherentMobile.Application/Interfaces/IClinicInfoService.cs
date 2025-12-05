using CoherentMobile.Application.DTOs.Clinic;

namespace CoherentMobile.Application.Interfaces;

/// <summary>
/// Service for managing clinic information (Guest Mode)
/// </summary>
public interface IClinicInfoService
{
    /// <summary>
    /// Get complete clinic information including doctors and services
    /// </summary>
    Task<ClinicInfoDto> GetClinicInfoAsync();
}
