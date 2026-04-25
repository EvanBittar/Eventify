using Eventify_High_Performance_Event_Management_API.Data;
using Eventify_High_Performance_Event_Management_API.Dtos;
using Eventify_High_Performance_Event_Management_API.Repository.Interfaces;

namespace Eventify_High_Performance_Event_Management_API.Repository
{
    public class BookingRepository : IBookingRepository
    {
        private readonly DataContext _dapper;

        public BookingRepository(DataContext data)
        {
            _dapper = data;
        }
        public async Task<IEnumerable<dynamic>> GetUserBookingsAsync(int userId)
        {
            string sql = @"SELECT b.BookingId , e.Title , e.StartDate , e.Location ,c.NameCategory, e.Price, ,
                    b.Status
                    FROM Event.Bookings AS b
                    JOIN Event.Events AS e ON b.EventId = e.EventId
                    JOIN Event.Categories AS c ON e.CategoryId = c.CategoryId
                    WHERE b.UserId = @UserId";

            return await _dapper.LoadData<dynamic>(sql, new { UserId = userId });
        }
        public async Task<string> CreateBookingAsync(BookingDto bookingDto)
        {
            string sql = @"
        IF NOT EXISTS (SELECT 1 FROM Event.Events WHERE EventId = @EventId)
            SELECT 'Event does not exist';
        ELSE IF EXISTS (SELECT 1 FROM Event.Bookings WHERE EventId = @EventId AND UserId = @UserId)
            SELECT 'Already Booked';
        ELSE IF (SELECT StartDate FROM Event.Events WHERE EventId = @EventId) <= GETDATE()
            SELECT 'Event has already passed';
        ELSE IF (SELECT COUNT(*) FROM Event.Bookings WHERE EventId = @EventId) >= (SELECT MaxAttendees FROM Event.Events WHERE EventId = @EventId)
            SELECT 'Event is full';
        ELSE
        BEGIN
            INSERT INTO Event.Bookings (EventId, UserId, BookingDate, Status) 
            VALUES (@EventId, @UserId, GETDATE(), '1');
            SELECT 'Success';
        END";

            var result = await _dapper.LoadDataSingle<string>(sql, bookingDto);
            return result ?? "Error processing booking";
        }
        public async Task<bool> CancelBookingAsync(int bookingId)
        {
            string sql = "DELETE FROM Event.Bookings WHERE BookingId = @BookingId";
            return await _dapper.ExecuteSql(sql, new { BookingId = bookingId });
        }
    }
}