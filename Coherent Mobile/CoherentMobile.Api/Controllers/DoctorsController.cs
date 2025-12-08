using System.Collections.Generic;
using System.Threading.Tasks;
using CoherentMobile.Application.DTOs.Clinic;
using CoherentMobile.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace CoherentMobile.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DoctorsController : ControllerBase
    {
        private readonly IClinicInfoService _clinicInfoService;
        private readonly ILogger<DoctorsController> _logger;

        public DoctorsController(IClinicInfoService clinicInfoService, ILogger<DoctorsController> logger)
        {
            _clinicInfoService = clinicInfoService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<DoctorDto>>> GetAllDoctors()
        {
            _logger.LogInformation("Fetching all doctors");

            try
            {
                var doctors = await _clinicInfoService.GetDoctorsAsync();
                return Ok(doctors);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching doctors");
                return StatusCode(500, "An error occurred while fetching doctors");
            }
        }
    }
}