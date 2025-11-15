namespace Shared.Models.Dtos
{
    public class JourneyMessageDto
    {
        public int Id { get; set; }
        public int JourneyId { get; set; }

        public int SenderId { get; set; }
        public string SenderName { get; set; } = string.Empty;

        public string Content { get; set; } = string.Empty;
        public DateTime SentAt { get; set; }
    }
}
