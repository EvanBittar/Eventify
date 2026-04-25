using Eventify_High_Performance_Event_Management_API.Models;

namespace Eventify_High_Performance_Event_Management_API.Repository.Interfaces
{
    public interface IUserRepository
    {
        Task<User?> GetUserByIdAsync(int id);
        Task<IEnumerable<User>> GetAllUsersAsync();
        Task<User?> GetUserByEmailAsync(string email);
        Task<bool> AddUserAsync(User user);
    }
}