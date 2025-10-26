namespace Server.Models
{
    public class JourneyMessages
    {
        public int Id { get; set; }
        public int JourneyId { get; set; }
        public Journey Journey { get; set; } = null!;

        public int SenderId { get; set; }
        public User Sender { get; set; } = null!;

        public string Content { get; set; } = string.Empty;
        public DateTime SentAt { get; set; }
    }
}
