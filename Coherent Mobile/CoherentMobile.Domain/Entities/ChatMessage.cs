namespace CoherentMobile.Domain.Entities
{
    public class ChatMessage
    {
        public int MessageId { get; set; }
        public int ConversationId { get; set; }
        public int SenderId { get; set; }
        public string SenderType { get; set; } = string.Empty; // Patient, Doctor, Staff
        public string MessageType { get; set; } = "Text"; // Text, Image, File, Voice, Video
        public string? Content { get; set; }
        public string? FileUrl { get; set; }
        public string? FileName { get; set; }
        public long? FileSize { get; set; }
        public DateTime SentAt { get; set; }
        public bool IsDelivered { get; set; }
        public bool IsRead { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime? DeletedAt { get; set; }
        public int? ReplyToMessageId { get; set; }

        // Navigation properties
        public Conversation Conversation { get; set; } = null!;
        public ChatMessage? ReplyToMessage { get; set; }
    }
}
