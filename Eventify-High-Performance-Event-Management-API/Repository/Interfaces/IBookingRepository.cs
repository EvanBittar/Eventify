using Eventify_High_Performance_Event_Management_API.Dtos;
using Eventify_High_Performance_Event_Management_API.Models;

namespace Eventify_High_Performance_Event_Management_API.Repository.Interfaces
{
    public interface IBookingRepository
    {
        Task<string> CreateBookingAsync(BookingDto bookingDto);
        Task<bool> CancelBookingAsync(int bookingId);
        Task<IEnumerable<dynamic>> GetUserBookingsAsync(int userId);
    }


}