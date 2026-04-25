using Eventify_High_Performance_Event_Management_API.Data;
using Eventify_High_Performance_Event_Management_API.Dtos;
using Eventify_High_Performance_Event_Management_API.Repository.Interfaces;

namespace Eventify_High_Performance_Event_Management_API.Repository
{
    public class ReviewRepository : IReviewRepository
    {
        private readonly DataContext _dapper;
        public ReviewRepository(DataContext data)
        {
            _dapper = data;
        }
        public async Task<string> AddReviewAsync(ReviewDto reviewDto, int userId)
        {
            string sql = @"IF EXISTS (SELECT 1 FROM Event.bookings WHERE EventId = @EventId AND UserId = @UserId)
                    BEGIN
                        INSERT INTO Event.Reviews (EventId, UserId, Rating, Comment)
                        VALUES (@EventId, @UserId, @Rating, @Comment);
                        SELECT 'Success';
                    END
                    ELSE 
                    BEGIN
                        SELECT 'You must book the event before reviewing it';
                    END";
            var result = await _dapper.LoadDataSingle<string>(sql, new
            {
                reviewDto.EventId,
                UserId = userId,
                reviewDto.Rating,
                reviewDto.Comment
            });
            return result ?? "Error processing review";
        }
        public async Task<IEnumerable<dynamic>> GetEventReviewsAsync(int eventId)
        {
            string sql = @"SELECT r.ReviewId, r.Rating, r.Comment, r.ReviewDate, u.Email AS UserEmail
                FROM Event.Reviews AS r
                JOIN Event.Users AS u ON r.UserId = u.UserId
                WHERE r.EventId = @EventId
                ORDER BY r.ReviewDate DESC";
            return await _dapper.LoadData<dynamic>(sql, new { EventId = eventId });
        }
    }
}