namespace CoherentMobile.Domain.Entities
{
    /// <summary>Row returned by CrmGetDoctorToPatientUpdatesAsync</summary>
    public class CrmMessageUpdateRow
    {
        public int ConversationId { get; set; }
        public int MessageId { get; set; }
        public DateTime SentAt { get; set; }
        public string DoctorLicenseNo { get; set; } = string.Empty;
        public string PatientMrNo { get; set; } = string.Empty;
        public string MessageType { get; set; } = string.Empty;
        public string? Content { get; set; }
        public string? FileUrl { get; set; }
        public string? FileName { get; set; }
        public long? FileSize { get; set; }
    }

    /// <summary>Counterpart info for a conversation list item</summary>
    public class CrmConversationCounterpart
    {
        public string UserType { get; set; } = string.Empty;
        public string? DoctorLicenseNo { get; set; }
        public string? DoctorName { get; set; }
        public string? DoctorTitle { get; set; }
        public string? DoctorPhotoName { get; set; }
        public string? PatientMrNo { get; set; }
        public string? PatientName { get; set; }
    }

    /// <summary>Row returned by CrmGetConversationListAsync</summary>
    public class CrmConversationListItem
    {
        public int ConversationId { get; set; }
        public DateTime? LastMessageAt { get; set; }
        public string? LastMessagePreview { get; set; }
        public int UnreadCount { get; set; }
        public CrmConversationCounterpart Counterpart { get; set; } = new();
    }

    /// <summary>Full response for conversation list</summary>
    public class CrmConversationListResult
    {
        public string? DoctorLicenseNo { get; set; }
        public string? PatientMrNo { get; set; }
        public DateTime ServerTimeUtc { get; set; } = DateTime.UtcNow;
        public List<CrmConversationListItem> Conversations { get; set; } = new();
    }

    /// <summary>Row returned by CrmGetThreadMessagesAsync</summary>
    public class CrmThreadMessageRow
    {
        public int MessageId { get; set; }
        public string SenderType { get; set; } = string.Empty;
        public string MessageType { get; set; } = string.Empty;
        public string? Content { get; set; }
        public string? FileUrl { get; set; }
        public string? FileName { get; set; }
        public long? FileSize { get; set; }
        public DateTime SentAt { get; set; }
    }
}
