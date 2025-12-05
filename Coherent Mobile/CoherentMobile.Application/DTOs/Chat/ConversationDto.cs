namespace CoherentMobile.Application.DTOs.Chat
{
    /// <summary>
    /// Conversation DTO for chat list
    /// </summary>
    public class ConversationDto
    {
        public int ConversationId { get; set; }
        public string ConversationType { get; set; } = string.Empty; // OneToOne, Group, Support
        public string? Title { get; set; }
        public DateTime? LastMessageAt { get; set; }
        public string? LastMessage { get; set; }
        public int UnreadCount { get; set; }
        public bool IsMuted { get; set; }
        
        // Other participant info (for one-to-one chats)
        public int? OtherUserId { get; set; }
        public string? OtherUserType { get; set; }
        public string? OtherUserName { get; set; }
        public string? OtherUserAvatar { get; set; }
        public bool? OtherUserIsOnline { get; set; }
        public DateTime? OtherUserLastSeen { get; set; }
    }
}
