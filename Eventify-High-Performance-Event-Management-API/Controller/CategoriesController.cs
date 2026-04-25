
using Eventify_High_Performance_Event_Management_API.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Eventify_High_Performance_Event_Management_API.Controller
{
    [ApiController]
    [Authorize]
    [Route("api/[controller]")]
    public class CategoriesController(IConfiguration config) : ControllerBase
    {
        private readonly DataContext _dapper = new(config);
        
        [HttpPost("AddCategory")]
        public async Task<IActionResult> AddCategory(string nameCategory)
        {
            var isAdmin = User.FindFirst("IsAdmin")?.Value;
            if (isAdmin != "True") return Forbid("Only admins can add categories.");

            string sql = @"INSERT INTO Event.Categories (NameCategory) VALUES (@NameCategory)";

            bool result = await _dapper.ExecuteSql(sql, new {NameCategory =  nameCategory});
            if (result) return Ok("Category added successfully.");
            return BadRequest("Failed to add category.");
        }

        [HttpDelete("DeleteCategory")]
        public async Task<IActionResult> DeleteCategory(int categoryId)
        {
            var isAdmin = User.FindFirst("IsAdmin")?.Value;
            if (isAdmin != "True") return Forbid("Only admins can delete categories.");

            if (categoryId <= 0 ) return BadRequest("Invalid category ID.");

            string checkSql = @"SELECT COUNT(*) FROM Event.Events WHERE CategoryId = @CategoryId";

            int eventCount = await _dapper.LoadDataSingle<int>(checkSql, new {CategoryId = categoryId});

            if (eventCount > 0) return BadRequest("Cannot delete category with associated events.");

            string sql = @"DELETE FROM Event.Categories WHERE CategoryId = @CategoryId";

            bool result = await _dapper.ExecuteSql(sql, new { CategoryId = categoryId });

            if(result) return Ok("Category deleted successfully.");
            return BadRequest("Failed to delete category.");  
        } 
    }
}