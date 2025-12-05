namespace CoherentMobile.Application.DTOs.Chat
{
    /// <summary>
    /// Response DTO for getting conversations list
    /// </summary>
    public class GetConversationsResponseDto
    {
        public List<ConversationDto> Conversations { get; set; } = new();
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
    }
}
