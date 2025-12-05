using System.Net.Http.Json;
using System.Text.Json;
using CoherentMobile.ExternalIntegration.Interfaces;
using CoherentMobile.ExternalIntegration.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace CoherentMobile.ExternalIntegration.Clients;

/// <summary>
/// Implementation of health data API client for third-party integration
/// Isolates external API calls from business logic
/// </summary>
public class HealthDataApiClient : IHealthDataApiClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<HealthDataApiClient> _logger;
    private readonly string _apiBaseUrl;
    private readonly string _apiKey;

    public HealthDataApiClient(
        HttpClient httpClient,
        IConfiguration configuration,
        ILogger<HealthDataApiClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        _apiBaseUrl = configuration["ExternalApis:HealthDataApi:BaseUrl"] ?? "https://api.healthdata.example.com";
        _apiKey = configuration["ExternalApis:HealthDataApi:ApiKey"] ?? string.Empty;

        // Configure HttpClient
        _httpClient.BaseAddress = new Uri(_apiBaseUrl);
        _httpClient.DefaultRequestHeaders.Add("X-API-Key", _apiKey);
        _httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
    }

    public async Task<ExternalHealthDataResponse?> GetHealthDataAsync(string userId, string dataType)
    {
        try
        {
            _logger.LogInformation("Fetching health data for user {UserId}, type {DataType}", userId, dataType);

            var response = await _httpClient.GetAsync($"/api/v1/health-data/{userId}/{dataType}");
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<ExternalHealthDataResponse>();
            
            _logger.LogInformation("Successfully retrieved health data for user {UserId}", userId);
            return result;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error fetching health data for user {UserId}", userId);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching health data for user {UserId}", userId);
            throw;
        }
    }

    public async Task<bool> SyncHealthDataAsync(string userId, HealthData healthData)
    {
        try
        {
            _logger.LogInformation("Syncing health data for user {UserId}", userId);

            var response = await _httpClient.PostAsJsonAsync($"/api/v1/health-data/{userId}/sync", healthData);
            response.EnsureSuccessStatusCode();

            _logger.LogInformation("Successfully synced health data for user {UserId}", userId);
            return true;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error syncing health data for user {UserId}", userId);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error syncing health data for user {UserId}", userId);
            return false;
        }
    }

    public async Task<IEnumerable<HealthData>> GetHistoricalDataAsync(string userId, DateTime startDate, DateTime endDate)
    {
        try
        {
            _logger.LogInformation("Fetching historical health data for user {UserId} from {StartDate} to {EndDate}", 
                userId, startDate, endDate);

            var queryString = $"?startDate={startDate:yyyy-MM-dd}&endDate={endDate:yyyy-MM-dd}";
            var response = await _httpClient.GetAsync($"/api/v1/health-data/{userId}/history{queryString}");
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<IEnumerable<HealthData>>() ?? 
                        Enumerable.Empty<HealthData>();

            _logger.LogInformation("Successfully retrieved {Count} historical records for user {UserId}", 
                result.Count(), userId);
            
            return result;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error fetching historical data for user {UserId}", userId);
            return Enumerable.Empty<HealthData>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching historical data for user {UserId}", userId);
            return Enumerable.Empty<HealthData>();
        }
    }
}
