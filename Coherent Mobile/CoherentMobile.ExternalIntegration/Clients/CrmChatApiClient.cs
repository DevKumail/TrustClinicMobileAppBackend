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
    public sealed class CrmChatApiClient : ICrmChatApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<CrmChatApiClient> _logger;
        private readonly string _v1BaseUrl;
        private readonly string _v2BaseUrl;

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public CrmChatApiClient(
            HttpClient httpClient,
            ILogger<CrmChatApiClient> logger,
            IOptions<CrmChatApiSettings> settings)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            var v1 = settings?.Value?.V1BaseUrl;
            var v2 = settings?.Value?.V2BaseUrl;
            if (string.IsNullOrWhiteSpace(v1) || string.IsNullOrWhiteSpace(v2))
            {
                throw new ArgumentNullException(nameof(settings), "CRM Chat API base URLs are not configured");
            }

            _v1BaseUrl = v1;
            _v2BaseUrl = v2;
        }

        private string ResolveBaseUrlForSend(CrmSendMessageRequest request)
        {
            if (!string.IsNullOrWhiteSpace(request.SenderType) &&
                request.SenderType.Equals("Doctor", StringComparison.OrdinalIgnoreCase))
            {
                return _v1BaseUrl;
            }

            return _v2BaseUrl;
        }

        public async Task<CrmGetOrCreateThreadResponse> GetOrCreateThreadAsync(CrmGetOrCreateThreadRequest request)
        {
            try
            {
                _logger.LogInformation(
                    "CRM Chat: GetOrCreateThread PatientMrNo={PatientMrNo}, DoctorLicenseNo={DoctorLicenseNo}",
                    request.PatientMrNo,
                    request.DoctorLicenseNo);

                var payload = JsonSerializer.Serialize(request, JsonOptions);
                using var content = new StringContent(payload, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync($"{_v2BaseUrl}/chat/threads/get-or-create", content);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError(
                        "CRM Chat GetOrCreateThread upstream failed. Status: {StatusCode}. Body: {Body}",
                        (int)response.StatusCode,
                        responseContent);
                    throw new ApplicationException(
                        $"Upstream CRM Chat GetOrCreateThread failed. StatusCode={(int)response.StatusCode}. Body={responseContent}");
                }

                var result = JsonSerializer.Deserialize<CrmGetOrCreateThreadResponse>(responseContent, JsonOptions);
                return result ?? new CrmGetOrCreateThreadResponse();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "CRM Chat: Error in GetOrCreateThread");
                throw new ApplicationException($"Failed to get or create thread: {ex.Message}", ex);
            }
        }

        public async Task<CrmSendMessageResponse> SendMessageAsync(CrmSendMessageRequest request)
        {
            try
            {
                _logger.LogInformation(
                    "CRM Chat: SendMessage Thread={CrmThreadId}, SenderType={SenderType}, ReceiverType={ReceiverType}, MessageType={MessageType}",
                    request.CrmThreadId,
                    request.SenderType,
                    request.ReceiverType,
                    request.MessageType);

                var payload = JsonSerializer.Serialize(request, JsonOptions);
                using var content = new StringContent(payload, Encoding.UTF8, "application/json");

                var baseUrl = ResolveBaseUrlForSend(request);
                var response = await _httpClient.PostAsync($"{baseUrl}/chat/messages", content);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError(
                        "CRM Chat SendMessage upstream failed. Status: {StatusCode}. Body: {Body}",
                        (int)response.StatusCode,
                        responseContent);
                    throw new ApplicationException(
                        $"Upstream CRM Chat SendMessage failed. StatusCode={(int)response.StatusCode}. Body={responseContent}");
                }

                var result = JsonSerializer.Deserialize<CrmSendMessageResponse>(responseContent, JsonOptions);
                return result ?? new CrmSendMessageResponse();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "CRM Chat: Error in SendMessage");
                throw new ApplicationException($"Failed to send message: {ex.Message}", ex);
            }
        }

        public async Task<IEnumerable<CrmMessageUpdateEvent>> GetMessageUpdatesAsync(DateTime since, int limit = 100)
        {
            try
            {
                _logger.LogInformation("CRM Chat: GetMessageUpdates Since={Since}, Limit={Limit}", since, limit);

                var sinceIso = Uri.EscapeDataString(since.ToString("O"));
                var response = await _httpClient.GetAsync(
                    $"{_v2BaseUrl}/chat/messages/updates?since={sinceIso}&limit={limit}");
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<List<CrmMessageUpdateEvent>>(content, JsonOptions);
                return result ?? new List<CrmMessageUpdateEvent>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "CRM Chat: Error in GetMessageUpdates");
                throw new ApplicationException($"Failed to get message updates: {ex.Message}", ex);
            }
        }
    }
}
