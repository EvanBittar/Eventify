namespace Eventify_High_Performance_Event_Management_API.Models
{
    public class Event
    {
        public int EventId { get; set; }
        public int CategoryId { get; set; }
        public int MaxAttendees { get; set; }
        public int CreatedBy { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal Price { get; set; }
        public decimal AverageRating { get; set; }
        public int ReviewsCount { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}