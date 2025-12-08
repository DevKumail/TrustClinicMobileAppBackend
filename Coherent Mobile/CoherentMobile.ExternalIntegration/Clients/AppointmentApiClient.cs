using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using CoherentMobile.ExternalIntegration.Interfaces;
using CoherentMobile.ExternalIntegration.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CoherentMobile.ExternalIntegration.Clients
{
    public class AppointmentApiClient : IAppointmentApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<AppointmentApiClient> _logger;
        private readonly string _baseUrl;

        public AppointmentApiClient(
            HttpClient httpClient, 
            ILogger<AppointmentApiClient> logger,
            IOptions<AppointmentsApiSettings> settings)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _baseUrl = settings?.Value?.BaseUrl ?? 
                throw new ArgumentNullException(nameof(settings), "Appointments API base URL is not configured");
        }

        public async Task<IEnumerable<Appointment>> GetAppointmentsByMrnoAsync(string mrno)
        {
            try
            {
                _logger.LogInformation("Fetching appointments for MRNO: {Mrno}", mrno);
                
                var response = await _httpClient.GetAsync($"{_baseUrl}/Appointments/GetAllAppointmentByMRNO?MRNO={Uri.EscapeDataString(mrno)}");
                response.EnsureSuccessStatusCode();
                
                var content = await response.Content.ReadAsStringAsync();
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };
                
                var appointments = JsonSerializer.Deserialize<List<Appointment>>(content, options);
                return appointments ?? new List<Appointment>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching appointments for MRNO: {Mrno}", mrno);
                throw new ApplicationException($"Failed to fetch appointments: {ex.Message}", ex);
            }
        }

        public async Task<DoctorSlotsApiResponse> GetAvailableDoctorSlotsAsync(int doctorId, string prsnlAlias, DateTime fromDate, DateTime toDate)
        {
            try
            {
                _logger.LogInformation("Fetching available slots for doctor: {DoctorId}, Alias: {PrsnlAlias}, From: {FromDate}, To: {ToDate}", 
                    doctorId, prsnlAlias, fromDate, toDate);

                var fromDateStr = fromDate.ToString("MM-dd-yyyy");
                var toDateStr = toDate.ToString("MM-dd-yyyy");
                
                var response = await _httpClient.GetAsync(
                    $"{_baseUrl}/Appointments/GetAvailableSlotOfDoctor?" +
                    $"doctorId={doctorId}&" +
                    $"prsnlAlias={Uri.EscapeDataString(prsnlAlias)}&" +
                    $"fromDate={fromDateStr}&" +
                    $"toDate={toDateStr}");

                response.EnsureSuccessStatusCode();
                
                var content = await response.Content.ReadAsStringAsync();
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                };
                
                var result = JsonSerializer.Deserialize<DoctorSlotsApiResponse>(content, options);
                return result ?? new DoctorSlotsApiResponse();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching available slots for doctor: {DoctorId}", doctorId);
                throw new ApplicationException($"Failed to fetch available slots: {ex.Message}", ex);
            }
        }
    }
}
