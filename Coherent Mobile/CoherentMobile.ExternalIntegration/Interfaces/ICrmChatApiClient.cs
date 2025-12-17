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
    }
}
