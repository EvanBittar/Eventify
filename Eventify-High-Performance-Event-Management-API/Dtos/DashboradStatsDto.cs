namespace Eventify_High_Performance_Event_Management_API.Dtos
{
    public class DashboardStatsDto
    {
        public decimal TotalRevenue { get; set; }
        public int TotalBookings { get; set; }
        public int ActiveEvents { get; set; }
        public int TotalUsers { get; set; }
        public string TopRatedEvent { get; set; } = "N/A";
    }
}