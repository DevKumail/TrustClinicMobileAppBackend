using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CoherentMobile.ExternalIntegration.Models;

namespace CoherentMobile.ExternalIntegration.Interfaces
{
    public interface ICrmChatApiClient
    {
        Task<CrmGetOrCreateThreadResponse> GetOrCreateThreadAsync(CrmGetOrCreateThreadRequest request);
        Task<CrmSendMessageResponse> SendMessageAsync(CrmSendMessageRequest request);
        Task<IEnumerable<CrmMessageUpdateEvent>> GetMessageUpdatesAsync(DateTime since, int limit = 100);
        Task<CrmConversationListResponse> GetConversationsAsync(string patientMrNo, int limit = 50);

        // Broadcast Channel APIs
        Task<CrmGetOrCreateBroadcastChannelResponse> GetOrCreateBroadcastChannelAsync(CrmGetOrCreateBroadcastChannelRequest request);
        Task<IEnumerable<CrmBroadcastChannelItem>> GetBroadcastChannelsAsync(string staffType, int limit = 50);
        Task<CrmStaffUnreadSummary> GetStaffUnreadSummaryAsync(string staffType);
        Task<IEnumerable<CrmThreadMessage>> GetThreadMessagesAsync(string crmThreadId, int take = 50);
        Task<CrmMarkReadResponse> MarkBroadcastChannelReadAsync(string crmThreadId, long empId, string staffType);
    }
}
