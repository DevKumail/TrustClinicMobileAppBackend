namespace CoherentMobile.Application.DTOs.Chat
{
    /// <summary>
    /// Request DTO for sending a message
    /// </summary>
    public class SendMessageRequestDto
    {
        public int ConversationId { get; set; }
        public string MessageType { get; set; } = "Text"; // Text, Image, File, Voice, Video
        public string? Content { get; set; }
        public string? FileUrl { get; set; }
        public string? FileName { get; set; }
        public long? FileSize { get; set; }
        public int? ReplyToMessageId { get; set; }
    }
}
