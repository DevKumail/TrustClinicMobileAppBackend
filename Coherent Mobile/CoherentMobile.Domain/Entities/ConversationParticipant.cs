namespace CoherentMobile.Domain.Entities
{
    public class ConversationParticipant
    {
        public int ParticipantId { get; set; }
        public int ConversationId { get; set; }
        public int UserId { get; set; }
        public string UserType { get; set; } = string.Empty; // Patient, Doctor, Staff
        public DateTime JoinedAt { get; set; }
        public DateTime? LeftAt { get; set; }
        public bool IsActive { get; set; }
        public bool IsMuted { get; set; }
        public int? LastReadMessageId { get; set; }
        public int UnreadCount { get; set; }

        // Navigation properties
        public Conversation Conversation { get; set; } = null!;
    }
}
