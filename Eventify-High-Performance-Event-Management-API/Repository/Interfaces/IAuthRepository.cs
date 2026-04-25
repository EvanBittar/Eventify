using Eventify_High_Performance_Event_Management_API.Models;

namespace Eventify_High_Performance_Event_Management_API.Repository.Interfaces
{
    public interface IAuthRepository
    {
        Task<bool> SaveVerificationCode(string email, string code, DateTime expiration);
        Task<User?> GetUserByResetCode(string email, string code);
        Task<bool> UpdatePassword(string email, string newPasswordHash);
        Task<bool> VerifyUserEmail(string email);
    }
}