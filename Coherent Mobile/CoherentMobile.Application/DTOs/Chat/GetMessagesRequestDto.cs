namespace CoherentMobile.Application.DTOs.Chat
{
    /// <summary>
    /// Request DTO for getting messages
    /// </summary>
    public class GetMessagesRequestDto
    {
        public int ConversationId { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 50;
    }
}
