using Eventify_High_Performance_Event_Management_API.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Eventify_High_Performance_Event_Management_API.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class AdminController(IConfiguration config) : ControllerBase
    {
        private readonly DataContext _dapper = new(config);
    [Authorize(Roles = "Admin")]
    [HttpGet("test")]
    public IActionResult test()
        {
            return Ok("it's work");
        } 
    [HttpGet("GetAdminStats")]
    public async Task<IActionResult> GetAdminStats()
        {
            var isAdmin = User.FindFirst("IsAdmin")?.Value;
            if (isAdmin != "True") return Forbid();

            string sql = @"SELECT 
                    (SELECT COUNT(*) FROM Event.Users) AS TotalUsers,
                    (SELECT COUNT(*) FROM Event.Events WHERE StartDate > GETDATE()) AS ActiveEvents,
                    (SELECT COUNT(*) FROM Event.Bookings WHERE Status = 1) AS TotalBookings";
            var stats = await _dapper.LoadDataSingle<dynamic>(sql, new{});
            return Ok(stats);
        }
    [HttpGet("MostBookedEvents")]
    public async Task<IActionResult> MostBookedEvents(int topN = 5)
        {
            var isAdmin = User.FindFirst("IsAdmin")?.Value;
            if (isAdmin != "True") return Forbid();

            string sql = @"SELECT TOP (5) 
                    e.Title, 
                    COUNT(b.BookingId) AS BookingCount
                FROM Event.Events AS e
                LEFT JOIN Event.Bookings AS b ON e.EventId = b.EventId
                GROUP BY e.EventId, e.Title
                ORDER BY BookingCount DESC";
            var result = await _dapper.LoadData<dynamic>(sql, new { TopN = topN });
            return Ok(result);
        }
    }
}