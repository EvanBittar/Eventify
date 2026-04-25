namespace Eventify_High_Performance_Event_Management_API.Dtos
{
    public class BookingDto
    {
        public int EventId { get; set; }
        public int UserId { get; set; }
        public DateTime BookingDate { get; set; }
        public short Status { get; set; } // 1: Confirmed, 2: Cancelled, 3: Attended
    }
}