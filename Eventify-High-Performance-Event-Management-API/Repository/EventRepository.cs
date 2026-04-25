using Eventify_High_Performance_Event_Management_API.Data;
using Eventify_High_Performance_Event_Management_API.Models;
using Eventify_High_Performance_Event_Management_API.Dtos;
using Eventify_High_Performance_Event_Management_API.Repository.Interfaces;

namespace Eventify_High_Performance_Event_Management_API.Repository
{
    public class EventRepository : IEventRepository
    {
        private readonly DataContext _dapper;

        public EventRepository(DataContext dapper)
        {
            _dapper = dapper;
        }

        public async Task<IEnumerable<dynamic>> GetAllEventsAsync()
        {
            string sql = @"SELECT e.*, c.NameCategory, u.Email as OrganizerEmail 
        FROM Event.Events e
        JOIN Event.Categories c ON e.CategoryId = c.CategoryId
        JOIN Event.Users u ON e.CreatedBy = u.UserId
        WHERE e.IsDeleted = 0";
            return await _dapper.LoadData<dynamic>(sql, new { });
        }

        public async Task<Event?> GetEventByIdAsync(int id)
        {
            string sql = "SELECT * FROM Event.Events WHERE EventId = @EventId AND IsDeleted = 0";
            return await _dapper.LoadDataSingle<Event>(sql, new { EventId = id });
        }

        public async Task<bool> CreateEventAsync(EventToAddDto eventToAddDto)
        {
            string sql = @"INSERT INTO Event.Events (Title, Description, Location, StartDate, EndDate, MaxAttendees, CategoryId, CreatedBy,Price)
                VALUES (@Title, @Description, @Location, @StartDate, @EndDate, @MaxAttendees, @CategoryId, @CreatedBy,@price)";

            return await _dapper.ExecuteSql(sql, eventToAddDto);
        }
        public async Task<IEnumerable<dynamic>> SearchEvents(string? title = null, int? CategoryId = null)
        {
            string sql = @"SELECT 
            e.EventId, e.Title, e.Location, e.Price, e.StartDate, c.NameCategory,
            ISNULL(AVG(CAST(r.Rating AS DECIMAL(10,2))), 0) AS AverageRating,
            COUNT(r.ReviewId) AS ReviewsCount
            FROM Event.Events AS e 
            JOIN Event.Categories AS c ON e.CategoryId = c.CategoryId
            LEFT JOIN Event.Reviews AS r ON e.EventId = r.EventId
            WHERE (@Title IS NULL OR e.Title LIKE '%' + @Title + '%')
            AND (@CategoryId IS NULL OR e.CategoryId = @CategoryId)
            GROUP BY e.EventId, e.Title, e.Location, e.Price, e.StartDate, c.NameCategory";

            return await _dapper.LoadData<dynamic>(sql, new { Title = title, CategoryId = CategoryId });

        }
        public async Task<bool> UpdateEventAsync(int id, EventToAddDto eventToAddDto)
        {
            string checkSql = "SELECT COUNT(*) FROM Event.Events WHERE EventId = @EventId";
            int count = await _dapper.LoadDataSingle<int>(checkSql, new { EventId = id });

            if (count == 0)
            {
                return false;
            }

            string sql = @"UPDATE Event.Events 
                SET Title = @Title, 
                    Description = @Description, 
                    Location = @Location, 
                    StartDate = @StartDate, 
                    EndDate = @EndDate, 
                    MaxAttendees = @MaxAttendees, 
                    CategoryId = @CategoryId, 
                    CreatedBy = @CreatedBy,
                    Price = @Price -- أضفنا السعر هنا
                WHERE EventId = @EventId";

            return await _dapper.ExecuteSql(sql, new
            {
                EventId = id,
                eventToAddDto.Title,
                eventToAddDto.Description,
                eventToAddDto.Location,
                eventToAddDto.StartDate,
                eventToAddDto.EndDate,
                eventToAddDto.MaxAttendees,
                eventToAddDto.CategoryId,
                eventToAddDto.CreatedBy,
                eventToAddDto.Price
            });
        }
        public async Task<DashboardStatsDto?> GetDashboardStatsAsync()
        {
            string sql = @"
        SELECT 
            (SELECT ISNULL(SUM(e.Price), 0) 
             FROM Event.Bookings b 
             JOIN Event.Events e ON b.EventId = e.EventId) AS TotalRevenue,
            
            (SELECT COUNT(*) FROM Event.Bookings) AS TotalBookings,
            
            (SELECT COUNT(*) FROM Event.Events WHERE StartDate > GETDATE()) AS ActiveEvents,
            
            (SELECT COUNT(*) FROM Event.Users) AS TotalUsers,
            
            ISNULL((SELECT TOP 1 e.Title 
              FROM Event.Events e
              JOIN Event.Reviews r ON e.EventId = r.EventId
              GROUP BY e.EventId, e.Title
              ORDER BY AVG(CAST(r.Rating AS DECIMAL)) DESC), 'No Reviews Yet') AS TopRatedEvent";

            return await _dapper.LoadDataSingle<DashboardStatsDto>(sql, new { });
        }
        public async Task<bool> DeleteEventAsync(int id)
        {
            string sql = @"UPDATE Event.Events 
                   SET IsDeleted = 1 
                   WHERE EventId = @Id";

            return await _dapper.ExecuteSql(sql, new { Id = id });
        }
    }
}