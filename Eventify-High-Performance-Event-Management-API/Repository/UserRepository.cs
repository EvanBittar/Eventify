using Eventify_High_Performance_Event_Management_API.Repository.Interfaces;
using Eventify_High_Performance_Event_Management_API.Models;
using Dapper;
using Microsoft.Data.SqlClient;
using Eventify_High_Performance_Event_Management_API.Data;

namespace Eventify_High_Performance_Event_Management_API.Repository
{
    public class UserRepository : IUserRepository
    {
        // private readonly string? _connectionString;
        private readonly DataContext _dapper;

        public UserRepository(IConfiguration configuration)
        {
            _dapper = new DataContext(configuration);
        }

        public async Task<User?> GetUserByIdAsync(int id) =>
            await _dapper.LoadDataSingle<User>(@"SELECT UserId, Email, PasswordHash, Roles, IsVerified
             FROM Event.Users WHERE UserId = @Id", new { Id = id });
        public async Task<IEnumerable<User>> GetAllUsersAsync() =>
            await _dapper.LoadData<User>("SELECT * FROM Event.Users");
        public async Task<User?> GetUserByEmailAsync(string email) =>
            await _dapper.LoadDataSingle<User>(@"SELECT UserId, Email, PasswordHash, Roles, IsVerified
             FROM Event.Users WHERE Email = @Email", new { Email = email });
        public async Task<bool> AddUserAsync(User user)
        {
            string sql = @"INSERT INTO Event.Users(FirstName ,LastName ,Email ,PasswordHash ,IsAdmin) VALUES(
                @FirstName, @LastName, @Email, @PasswordHash, @IsAdmin)";
            return await _dapper.ExecuteSql(sql, user);
        }
    }
}