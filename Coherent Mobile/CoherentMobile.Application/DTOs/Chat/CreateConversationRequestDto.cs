namespace CoherentMobile.Application.DTOs.Chat
{
    /// <summary>
    /// Request DTO for creating a new conversation
    /// </summary>
    public class CreateConversationRequestDto
    {
        public int OtherUserId { get; set; }
        public string OtherUserType { get; set; } = string.Empty; // Patient, Doctor, Staff
        public string? InitialMessage { get; set; }
    }
}
