using System.Collections.Generic;
using System.Threading.Tasks;
using CoherentMobile.API.Models;
using CoherentMobile.Application.DTOs.Clinic;
using CoherentMobile.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace CoherentMobile.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DoctorsController : ControllerBase
    {
        private readonly IDoctorService _doctorService;
        private readonly ILogger<DoctorsController> _logger;

        public DoctorsController(IDoctorService doctorService, ILogger<DoctorsController> logger)
        {
            _doctorService = doctorService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<DoctorDto>>> GetAllDoctors()
        {
            _logger.LogInformation("Fetching all doctors");

            try
            {
                var doctors = await _doctorService.GetAllDoctorsAsync();

                var result = new List<DoctorDto>();
                foreach (var doctor in doctors)
                {
                    result.Add(new DoctorDto
                    {
                        Id = doctor.DoctorId,
                        Name = doctor.Name,
                        Specialization = doctor.Specialization,
                        Email = doctor.Email,
                        Phone = doctor.Phone
                    });
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching doctors");
                return StatusCode(500, "An error occurred while fetching doctors");
            }
        }
    }
}