namespace CoherentMobile.Application.DTOs.Chat
{
    /// <summary>
    /// Chat message DTO
    /// </summary>
    public class ChatMessageDto
    {
        public int MessageId { get; set; }
        public int ConversationId { get; set; }
        public int SenderId { get; set; }
        public string SenderType { get; set; } = string.Empty; // Patient, Doctor, Staff
        public string SenderName { get; set; } = string.Empty;
        public string? SenderAvatar { get; set; }
        public string MessageType { get; set; } = "Text"; // Text, Image, File, Voice, Video
        public string? Content { get; set; }
        public string? FileUrl { get; set; }
        public string? FileName { get; set; }
        public long? FileSize { get; set; }
        public DateTime SentAt { get; set; }
        public bool IsDelivered { get; set; }
        public bool IsRead { get; set; }
        public int? ReplyToMessageId { get; set; }
        public ChatMessageDto? ReplyToMessage { get; set; }
        
        // CRM sync fields
        public string? CrmMessageId { get; set; }
        public string? CrmThreadId { get; set; }
    }
}
