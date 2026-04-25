using Eventify_High_Performance_Event_Management_API.Data;
using Eventify_High_Performance_Event_Management_API.Models;
using Eventify_High_Performance_Event_Management_API.Repository.Interfaces;

namespace Eventify_High_Performance_Event_Management_API.Repository
{
    public class AuthRepository : IAuthRepository
    {
        private readonly DataContext _dapper;

        public AuthRepository(DataContext data)
        {
            _dapper = data;
        }
        public async Task<bool> SaveVerificationCode(string email, string code, DateTime expiration)
        {
            string sql = @"UPDATE Event.Users
                           SET VerificationCode = @Code, CodeExpiration = @Expiration
                           WHERE Email = @Email";
            return await _dapper.ExecuteSql(sql, new { Email = email, Code = code, Expiration = expiration });
        }
        public async Task<User?> GetUserByResetCode(string email, string code)
        {
            string sql = @"SELECT * FROM Event.Users 
                            WHERE Email = @Email AND VerificationCode = @Code
                            AND CodeExpiration > GETDATE()";
            return await _dapper.LoadDataSingle<User>(sql, new { Email = email, Code = code });
        }
        public async Task<bool> UpdatePassword(string email, string newPasswordHash)
        {
            string sql = @"UPDATE Event.Users
                           SET PasswordHash = @PasswordHash,
                            VerificationCode = NULL,
                            CodeExpiration = NULL
                           WHERE Email = @Email";
            return await _dapper.ExecuteSql(sql, new { Email = email, PasswordHash = newPasswordHash });
        }
        public async Task<bool> VerifyUserEmail(string email)
        {
            string sql = @"UPDATE Event.Users SET IsVerified = 1,
                             VerificationCode = NULL, 
                             CodeExpiration = NULL
                            WHERE Email = @Email";
            return await _dapper.ExecuteSql(sql, new { Email = email });
        }
    }
}