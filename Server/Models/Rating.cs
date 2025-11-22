namespace Server.Models
{
    public class Rating
    {
        public int JourneyId { get; set; }
        public Journey Journey { get; set; } = null!;
        public int UserId { get; set; }
        public User User { get; set; } = null!;
        public int ratingValue { get; set; }
        public string? Note { get; set; } = null;
        public DateTime? Created { get; set; } = DateTime.UtcNow;

    }
}
