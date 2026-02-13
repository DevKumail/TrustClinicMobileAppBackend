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

        public async Task<CrmConversationListResponse> GetConversationsAsync(string patientMrNo, int limit = 50)
        {
            try
            {
                _logger.LogInformation(
                    "CRM Chat: GetConversations PatientMrNo={PatientMrNo}, Limit={Limit}",
                    patientMrNo,
                    limit);

                if (string.IsNullOrWhiteSpace(patientMrNo))
                {
                    throw new ArgumentException("patientMrNo is required", nameof(patientMrNo));
                }

                var query = new List<string>();
                query.Add($"patientMrNo={Uri.EscapeDataString(patientMrNo)}");

                query.Add($"limit={limit}");

                var url = $"{_v2BaseUrl}/chat/conversations?{string.Join("&", query)}";

                var response = await _httpClient.GetAsync(url);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError(
                        "CRM Chat GetConversations upstream failed. Status: {StatusCode}. Body: {Body}",
                        (int)response.StatusCode,
                        responseContent);
                    throw new ApplicationException(
                        $"Upstream CRM Chat GetConversations failed. StatusCode={(int)response.StatusCode}. Body={responseContent}");
                }

                var result = JsonSerializer.Deserialize<CrmConversationListResponse>(responseContent, JsonOptions);
                var safeResult = result ?? new CrmConversationListResponse();

                foreach (var c in safeResult.Conversations)
                {
                    if (string.IsNullOrWhiteSpace(c.ConversationId))
                    {
                        c.ConversationId = c.CrmThreadId;
                    }
                }

                return safeResult;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "CRM Chat: Error in GetConversations");
                throw new ApplicationException($"Failed to get conversations: {ex.Message}", ex);
            }
        }

        public async Task<CrmGetOrCreateBroadcastChannelResponse> GetOrCreateBroadcastChannelAsync(CrmGetOrCreateBroadcastChannelRequest request)
        {
            try
            {
                _logger.LogInformation(
                    "CRM Chat: GetOrCreateBroadcastChannel PatientMrNo={PatientMrNo}, StaffType={StaffType}",
                    request.PatientMrNo,
                    request.StaffType);

                var payload = JsonSerializer.Serialize(request, JsonOptions);
                using var content = new StringContent(payload, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync($"{_v2BaseUrl}/chat/broadcast-channels/get-or-create", content);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError(
                        "CRM Chat GetOrCreateBroadcastChannel upstream failed. Status: {StatusCode}. Body: {Body}",
                        (int)response.StatusCode,
                        responseContent);
                    throw new ApplicationException(
                        $"Upstream CRM Chat GetOrCreateBroadcastChannel failed. StatusCode={(int)response.StatusCode}. Body={responseContent}");
                }

                var result = JsonSerializer.Deserialize<CrmGetOrCreateBroadcastChannelResponse>(responseContent, JsonOptions);
                return result ?? new CrmGetOrCreateBroadcastChannelResponse();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "CRM Chat: Error in GetOrCreateBroadcastChannel");
                throw new ApplicationException($"Failed to get or create broadcast channel: {ex.Message}", ex);
            }
        }

        public async Task<IEnumerable<CrmBroadcastChannelItem>> GetBroadcastChannelsAsync(string staffType, int limit = 50)
        {
            try
            {
                _logger.LogInformation(
                    "CRM Chat: GetBroadcastChannels StaffType={StaffType}, Limit={Limit}",
                    staffType,
                    limit);

                if (string.IsNullOrWhiteSpace(staffType))
                {
                    throw new ArgumentException("staffType is required", nameof(staffType));
                }

                var url = $"{_v2BaseUrl}/chat/broadcast-channels?staffType={Uri.EscapeDataString(staffType)}&limit={limit}";

                var response = await _httpClient.GetAsync(url);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError(
                        "CRM Chat GetBroadcastChannels upstream failed. Status: {StatusCode}. Body: {Body}",
                        (int)response.StatusCode,
                        responseContent);
                    throw new ApplicationException(
                        $"Upstream CRM Chat GetBroadcastChannels failed. StatusCode={(int)response.StatusCode}. Body={responseContent}");
                }

                var result = JsonSerializer.Deserialize<List<CrmBroadcastChannelItem>>(responseContent, JsonOptions);
                return result ?? new List<CrmBroadcastChannelItem>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "CRM Chat: Error in GetBroadcastChannels");
                throw new ApplicationException($"Failed to get broadcast channels: {ex.Message}", ex);
            }
        }

        public async Task<CrmStaffUnreadSummary> GetStaffUnreadSummaryAsync(string staffType)
        {
            try
            {
                _logger.LogInformation("CRM Chat: GetStaffUnreadSummary StaffType={StaffType}", staffType);

                if (string.IsNullOrWhiteSpace(staffType))
                {
                    throw new ArgumentException("staffType is required", nameof(staffType));
                }

                var url = $"{_v2BaseUrl}/chat/broadcast-channels/unread-summary?staffType={Uri.EscapeDataString(staffType)}";

                var response = await _httpClient.GetAsync(url);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError(
                        "CRM Chat GetStaffUnreadSummary upstream failed. Status: {StatusCode}. Body: {Body}",
                        (int)response.StatusCode,
                        responseContent);
                    throw new ApplicationException(
                        $"Upstream CRM Chat GetStaffUnreadSummary failed. StatusCode={(int)response.StatusCode}. Body={responseContent}");
                }

                var result = JsonSerializer.Deserialize<CrmStaffUnreadSummary>(responseContent, JsonOptions);
                return result ?? new CrmStaffUnreadSummary();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "CRM Chat: Error in GetStaffUnreadSummary");
                throw new ApplicationException($"Failed to get staff unread summary: {ex.Message}", ex);
            }
        }

        public async Task<IEnumerable<CrmThreadMessage>> GetThreadMessagesAsync(string crmThreadId, int take = 50)
        {
            try
            {
                _logger.LogInformation(
                    "CRM Chat: GetThreadMessages CrmThreadId={CrmThreadId}, Take={Take}",
                    crmThreadId,
                    take);

                if (string.IsNullOrWhiteSpace(crmThreadId))
                {
                    throw new ArgumentException("crmThreadId is required", nameof(crmThreadId));
                }

                var url = $"{_v2BaseUrl}/chat/threads/{Uri.EscapeDataString(crmThreadId)}/messages?take={take}";

                var response = await _httpClient.GetAsync(url);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError(
                        "CRM Chat GetThreadMessages upstream failed. Status: {StatusCode}. Body: {Body}",
                        (int)response.StatusCode,
                        responseContent);
                    throw new ApplicationException(
                        $"Upstream CRM Chat GetThreadMessages failed. StatusCode={(int)response.StatusCode}. Body={responseContent}");
                }

                var result = JsonSerializer.Deserialize<List<CrmThreadMessage>>(responseContent, JsonOptions);
                return result ?? new List<CrmThreadMessage>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "CRM Chat: Error in GetThreadMessages");
                throw new ApplicationException($"Failed to get thread messages: {ex.Message}", ex);
            }
        }

        public async Task<CrmMarkReadResponse> MarkBroadcastChannelReadAsync(string crmThreadId, long empId, string staffType)
        {
            try
            {
                _logger.LogInformation(
                    "CRM Chat: MarkBroadcastChannelRead CrmThreadId={CrmThreadId}, EmpId={EmpId}, StaffType={StaffType}",
                    crmThreadId,
                    empId,
                    staffType);

                if (string.IsNullOrWhiteSpace(crmThreadId))
                {
                    throw new ArgumentException("crmThreadId is required", nameof(crmThreadId));
                }

                var url = $"{_v2BaseUrl}/chat/broadcast-channels/{Uri.EscapeDataString(crmThreadId)}/mark-read?empId={empId}&staffType={Uri.EscapeDataString(staffType)}";

                var response = await _httpClient.PostAsync(url, null);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError(
                        "CRM Chat MarkBroadcastChannelRead upstream failed. Status: {StatusCode}. Body: {Body}",
                        (int)response.StatusCode,
                        responseContent);
                    throw new ApplicationException(
                        $"Upstream CRM Chat MarkBroadcastChannelRead failed. StatusCode={(int)response.StatusCode}. Body={responseContent}");
                }

                var result = JsonSerializer.Deserialize<CrmMarkReadResponse>(responseContent, JsonOptions);
                return result ?? new CrmMarkReadResponse();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "CRM Chat: Error in MarkBroadcastChannelRead");
                throw new ApplicationException($"Failed to mark broadcast channel as read: {ex.Message}", ex);
            }
        }
    }
}
