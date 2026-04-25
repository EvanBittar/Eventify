using System.Data;
using Dapper;
using Microsoft.Data.SqlClient;

namespace Eventify_High_Performance_Event_Management_API.Data
{

    public class DataContext
    {
        private readonly IConfiguration _config;
        private readonly string? _connectionString;
        public DataContext(IConfiguration config)
        {
            _config = config;
            _connectionString = _config.GetConnectionString("DefaultConnection");
        }

        public async Task<IEnumerable<T>> LoadData<T>(string sql , object? sqlParameter = null)
        {
            using IDbConnection db = new SqlConnection(_connectionString);
            return await  db.QueryAsync<T>(sql,sqlParameter);
        }
        public async Task<T?> LoadDataSingle<T>(string sql, object? sqlParameter = null)
        {
            using IDbConnection db = new SqlConnection(_connectionString);
        return await db.QuerySingleOrDefaultAsync<T>(sql, sqlParameter);
        }
        public async Task<bool> ExecuteSql(string sql, object? sqlParameter = null)
        {
            using IDbConnection db = new SqlConnection(_connectionString);
            return await db.ExecuteAsync(sql, sqlParameter) > 0;
        }
        public async Task<int> ExecuteSqlWithCountRow(string sql, object? sqlParameter = null)
        {
            using IDbConnection db = new SqlConnection(_connectionString);
            return await db.ExecuteAsync(sql, sqlParameter);
        }
    }
}