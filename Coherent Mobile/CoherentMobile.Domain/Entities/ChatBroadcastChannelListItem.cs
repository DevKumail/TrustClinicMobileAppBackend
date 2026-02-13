namespace CoherentMobile.Domain.Entities
{
    public sealed class ChatBroadcastChannelListItem
    {
        public int ConversationId { get; set; }
        public string? ChannelTitle { get; set; }
        public DateTime? LastMessageAt { get; set; }
        public string? LastMessagePreview { get; set; }
        public string PatientMrNo { get; set; } = string.Empty;
        public string? PatientName { get; set; }
        public int UnreadCount { get; set; }
    }
}
