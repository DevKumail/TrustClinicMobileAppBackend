using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
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

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

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

                var appointments = JsonSerializer.Deserialize<List<Appointment>>(content, JsonOptions);
                return appointments ?? new List<Appointment>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching appointments for MRNO: {Mrno}", mrno);
                throw new ApplicationException($"Failed to fetch appointments: {ex.Message}", ex);
            }
        }

        public async Task<DoctorSlotsApiResponse> GetAvailableDoctorSlotsAsync(string prsnlAlias, DateTime fromDate, DateTime toDate)
        {
            try
            {
                _logger.LogInformation("Fetching available slots for doctor: {DoctorId}, Alias: {PrsnlAlias}, From: {FromDate}, To: {ToDate}", 
                     prsnlAlias, fromDate, toDate);

                var fromDateStr = fromDate.ToString("MM-dd-yyyy");
                var toDateStr = toDate.ToString("MM-dd-yyyy");
                
                var response = await _httpClient.GetAsync(
                    $"{_baseUrl}/Appointments/GetAvailableSlotOfDoctor?" +
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
                _logger.LogError(ex, "Error fetching available slots for doctor: {prsnlAlias}", prsnlAlias);
                throw new ApplicationException($"Failed to fetch available slots: {ex.Message}", ex);
            }
        }

        public async Task<BookAppointmentResponse> BookAppointmentAsync(BookAppointmentRequest request)
        {
            try
            {
                _logger.LogInformation("Booking appointment for MRNO: {MrNo}, DoctorID: {DoctorID}", request.MrNo, request.DoctorID);

                var payload = JsonSerializer.Serialize(request, JsonOptions);
                using var content = new StringContent(payload, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync($"{_baseUrl}/Appointments/BookAppointment", content);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError(
                        "BookAppointment upstream failed. Status: {StatusCode}. Body: {Body}",
                        (int)response.StatusCode,
                        responseContent);
                    throw new ApplicationException($"Upstream BookAppointment failed. StatusCode={(int)response.StatusCode}. Body={responseContent}");
                }

                var result = JsonSerializer.Deserialize<BookAppointmentResponse>(responseContent, JsonOptions);
                return result ?? new BookAppointmentResponse();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error booking appointment for MRNO: {MrNo}", request.MrNo);
                throw new ApplicationException($"Failed to book appointment: {ex.Message}", ex);
            }
        }

        public async Task<CancelAppointmentResponse> CancelAppointmentAsync(CancelAppointmentRequest request)
        {
            try
            {
                _logger.LogInformation("Cancelling appointment for AppBookingId: {AppBookingId}", request.AppBookingId);

                var payload = JsonSerializer.Serialize(request, JsonOptions);
                using var content = new StringContent(payload, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync($"{_baseUrl}/Appointments/CancelAppointment", content);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError(
                        "CancelAppointment upstream failed. Status: {StatusCode}. Body: {Body}",
                        (int)response.StatusCode,
                        responseContent);
                    throw new ApplicationException($"Upstream CancelAppointment failed. StatusCode={(int)response.StatusCode}. Body={responseContent}");
                }

                var result = JsonSerializer.Deserialize<CancelAppointmentResponse>(responseContent, JsonOptions);
                return result ?? new CancelAppointmentResponse();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelling appointment for AppBookingId: {AppBookingId}", request.AppBookingId);
                throw new ApplicationException($"Failed to cancel appointment: {ex.Message}", ex);
            }
        }

        public async Task<ChangeBookedAppointmentResponse> ChangeBookedAppointmentAsync(ChangeBookedAppointmentRequest request)
        {
            try
            {
                _logger.LogInformation("Changing booked appointment AppId: {AppId} for MRNO: {MrNo}", request.AppId, request.MrNo);

                var payload = JsonSerializer.Serialize(request, JsonOptions);
                using var content = new StringContent(payload, Encoding.UTF8, "application/json");

                var response = await _httpClient.PutAsync($"{_baseUrl}/Appointments/ChangeBookedAppointment", content);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError(
                        "ChangeBookedAppointment upstream failed. Status: {StatusCode}. Body: {Body}",
                        (int)response.StatusCode,
                        responseContent);
                    throw new ApplicationException($"Upstream ChangeBookedAppointment failed. StatusCode={(int)response.StatusCode}. Body={responseContent}");
                }

                var result = JsonSerializer.Deserialize<ChangeBookedAppointmentResponse>(responseContent, JsonOptions);
                return result ?? new ChangeBookedAppointmentResponse();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error changing booked appointment AppId: {AppId}", request.AppId);
                throw new ApplicationException($"Failed to change booked appointment: {ex.Message}", ex);
            }
        }
    }
}
