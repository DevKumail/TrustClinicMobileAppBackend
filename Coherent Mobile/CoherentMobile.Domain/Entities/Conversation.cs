namespace CoherentMobile.Domain.Entities
{
    public class Conversation
    {
        public int ConversationId { get; set; }
        public string ConversationType { get; set; } = string.Empty; // OneToOne, Group, Support
        public string? Title { get; set; }
        public int CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastMessageAt { get; set; }
        public string? LastMessage { get; set; }
        public bool IsActive { get; set; }
        public bool IsArchived { get; set; }

        // Navigation properties
        public ICollection<ConversationParticipant> Participants { get; set; } = new List<ConversationParticipant>();
        public ICollection<ChatMessage> Messages { get; set; } = new List<ChatMessage>();
    }
}
