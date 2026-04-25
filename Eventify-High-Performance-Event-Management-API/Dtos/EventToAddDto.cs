namespace Eventify_High_Performance_Event_Management_API.Dtos
{
    public class EventToAddDto
    {
        public int CategoryId { get; set; }
        public int MaxAttendees { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal Price { get; set; }
        public int CreatedBy { get; set; }
    }
}