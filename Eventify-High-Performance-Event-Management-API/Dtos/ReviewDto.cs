namespace Eventify_High_Performance_Event_Management_API.Dtos
{
    public class ReviewDto
    {
        public int EventId { get; set; }
        public int UserId { get; set; }
        public int Rating { get; set; }
        public string Comment { get; set; } = string.Empty;
    }
}