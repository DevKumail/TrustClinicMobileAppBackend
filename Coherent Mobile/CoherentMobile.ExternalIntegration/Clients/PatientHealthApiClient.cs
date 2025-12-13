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
    public class PatientHealthApiClient : IPatientHealthApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<PatientHealthApiClient> _logger;
        private readonly string _baseUrl;

        public PatientHealthApiClient(
            HttpClient httpClient,
            ILogger<PatientHealthApiClient> logger,
            IOptions<PatientHealthApiSettings> settings)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _baseUrl = settings?.Value?.BaseUrl ??
                throw new ArgumentNullException(nameof(settings), "PatientHealth API base URL is not configured");
        }

        public async Task<IEnumerable<Medication>> GetMedicationsByMrnoV2Async(string mrno)
        {
            try
            {
                _logger.LogInformation("Fetching medications for MRNO: {Mrno}", mrno);

                var response = await _httpClient.GetAsync($"{_baseUrl}/PatientHealth/GetMedicationsByMRNOV2?MRNO={Uri.EscapeDataString(mrno)}");
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                var medications = JsonSerializer.Deserialize<List<Medication>>(content, options);
                return medications ?? new List<Medication>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching medications for MRNO: {Mrno}", mrno);
                throw new ApplicationException($"Failed to fetch medications: {ex.Message}", ex);
            }
        }

        public async Task<IEnumerable<Allergy>> GetAllergiesByMrnoAsync(string mrno)
        {
            try
            {
                _logger.LogInformation("Fetching allergies for MRNO: {Mrno}", mrno);

                var response = await _httpClient.GetAsync($"{_baseUrl}/PatientHealth/GetAllergyByMRNO?MRNO={Uri.EscapeDataString(mrno)}");
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                var allergies = JsonSerializer.Deserialize<List<Allergy>>(content, options);
                return allergies ?? new List<Allergy>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching allergies for MRNO: {Mrno}", mrno);
                throw new ApplicationException($"Failed to fetch allergies: {ex.Message}", ex);
            }
        }

        public async Task<IEnumerable<Diagnosis>> GetDiagnosesByMrnoAsync(string mrno)
        {
            try
            {
                _logger.LogInformation("Fetching diagnoses for MRNO: {Mrno}", mrno);

                var response = await _httpClient.GetAsync($"{_baseUrl}/PatientHealth/GetDiagnosisByMRNO?MRNO={Uri.EscapeDataString(mrno)}");
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                var diagnoses = JsonSerializer.Deserialize<List<Diagnosis>>(content, options);
                return diagnoses ?? new List<Diagnosis>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching diagnoses for MRNO: {Mrno}", mrno);
                throw new ApplicationException($"Failed to fetch diagnoses: {ex.Message}", ex);
            }
        }
    }
}
