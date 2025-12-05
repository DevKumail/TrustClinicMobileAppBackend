using CoherentMobile.Application.DTOs.Clinic;
using CoherentMobile.Application.Interfaces;
using CoherentMobile.Domain.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace CoherentMobile.Application.Services;

/// <summary>
/// Service implementation for clinic information (Guest Mode)
/// </summary>
public class ClinicInfoService : IClinicInfoService
{
    private readonly IFacilityRepository _facilityRepo;
    private readonly IDoctorRepository _doctorRepo;
    private readonly IServiceRepository _serviceRepo;
    private readonly ISpecialityRepository _specialityRepo;
    private readonly IConfiguration _configuration;
    private readonly ILogger<ClinicInfoService> _logger;

    public ClinicInfoService(
        IFacilityRepository facilityRepo,
        IDoctorRepository doctorRepo,
        IServiceRepository serviceRepo,
        ISpecialityRepository specialityRepo,
        IConfiguration configuration,
        ILogger<ClinicInfoService> logger)
    {
        _facilityRepo = facilityRepo;
        _doctorRepo = doctorRepo;
        _serviceRepo = serviceRepo;
        _specialityRepo = specialityRepo;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<ClinicInfoDto> GetClinicInfoAsync()
    {
        try
        {
            // Facility ID (default to first facility)
            const int facilityId = 1;
            
            // Get image base URL from configuration
            var imageBaseUrl = _configuration["ImageSettings:BaseUrl"] ?? "https://localhost:7162/images";
            var doctorImagesPath = _configuration["ImageSettings:DoctorImagesPath"] ?? "doctors";
            var facilityImagesPath = _configuration["ImageSettings:FacilityImagesPath"] ?? "facilities";
            var serviceImagesPath = _configuration["ImageSettings:ServiceImagesPath"] ?? "services";
            var serviceIconsPath = _configuration["ImageSettings:ServiceIconsPath"] ?? "icons";

            // Fetch facility information
            var facility = await _facilityRepo.GetByIdAsync(facilityId);
            if (facility == null)
            {
                _logger.LogWarning("Facility with ID {FacilityId} not found", facilityId);
                throw new Exception("Clinic information not found");
            }

            // Get all facilities for locations
            var allFacilities = await _facilityRepo.GetAllActiveAsync();
            var locations = allFacilities
                .Where(f => !string.IsNullOrEmpty(f.City) && !string.IsNullOrEmpty(f.Country))
                .Select(f => $"{f.City}, {f.Country}")
                .Distinct()
                .ToList();

            // Parse facility images (comma-separated)
            var facilityImages = string.IsNullOrEmpty(facility.FacilityImages)
                ? new List<string>()
                : facility.FacilityImages
                    .Split(',')
                    .Select(img => $"{imageBaseUrl}/{facilityImagesPath}/{img.Trim()}")
                    .ToList();

            // Fetch doctors for this facility
            var doctors = await _doctorRepo.GetByFacilityIdAsync(facilityId);
            var doctorDtos = new List<DoctorDto>();

            foreach (var doctor in doctors)
            {
                // Get speciality name
                var speciality = doctor.SPId.HasValue 
                    ? await _specialityRepo.GetByIdAsync(doctor.SPId.Value)
                    : null;

                // Parse languages (comma-separated)
                var languages = string.IsNullOrEmpty(doctor.Languages)
                    ? new List<string>()
                    : doctor.Languages.Split(',').Select(l => l.Trim()).ToList();

                // Get doctor location
                var doctorLocation = new List<string> { facility.City ?? "Abu Dhabi" };

                doctorDtos.Add(new DoctorDto
                {
                    Id = doctor.DId,
                    Name = doctor.DoctorName ?? string.Empty,
                    Position = doctor.Title ?? string.Empty,
                    Speciality = speciality?.SpecilityName ?? string.Empty,
                    ProvNPI = doctor.LicenceNo ?? string.Empty,
                    Experience = !string.IsNullOrEmpty(doctor.YearsOfExperience) 
                        ? $"{doctor.YearsOfExperience} Years of Experience" 
                        : string.Empty,
                    Nationality = doctor.Nationality ?? string.Empty,
                    Languages = languages,
                    Location = doctorLocation,
                    ImageUrl = !string.IsNullOrEmpty(doctor.DoctorPhotoName) 
                        ? $"{imageBaseUrl}/{doctorImagesPath}/{doctor.DoctorPhotoName}" 
                        : string.Empty,
                    Biography = doctor.About ?? string.Empty
                });
            }

            // Fetch services for this facility
            var services = await _serviceRepo.GetByFacilityIdAsync(facilityId);
            var serviceDtos = new List<ServiceDto>();

            foreach (var service in services)
            {
                // Fetch sub-services (Q&A) for this service
                var subServices = await _serviceRepo.GetSubServicesByServiceIdAsync(service.SId);
                var qnaDtos = subServices.Select(ss => new ServiceQnaDto
                {
                    Id = ss.SSId,
                    Question = ss.SubServiceTitle ?? string.Empty,
                    Answer = ss.Details ?? string.Empty
                }).ToList();

                serviceDtos.Add(new ServiceDto
                {
                    Id = service.SId,
                    ServiceIcon = !string.IsNullOrEmpty(service.IconImageName) 
                        ? $"{imageBaseUrl}/{serviceIconsPath}/{service.IconImageName}" 
                        : string.Empty,
                    ServiceName = service.ServiceTitle ?? string.Empty,
                    ServiceImage = !string.IsNullOrEmpty(service.DisplayImageName) 
                        ? $"{imageBaseUrl}/{serviceImagesPath}/{service.DisplayImageName}" 
                        : string.Empty,
                    Qna = qnaDtos
                });
            }

            // Build response DTO
            var clinicInfo = new ClinicInfoDto
            {
                Id = facility.FId,
                ClinicsImages = facilityImages,
                Locations = locations,
                Description = facility.About ?? string.Empty,
                Doctors = doctorDtos,
                Services = serviceDtos
            };

            _logger.LogInformation("Clinic information retrieved successfully. Doctors: {DoctorCount}, Services: {ServiceCount}", 
                doctorDtos.Count, serviceDtos.Count);

            return clinicInfo;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving clinic information");
            throw;
        }
    }
}
