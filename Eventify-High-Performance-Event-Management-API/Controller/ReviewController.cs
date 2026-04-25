using System.Security.Claims;
using Eventify_High_Performance_Event_Management_API.Dtos;
using Eventify_High_Performance_Event_Management_API.Repository.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Eventify_High_Performance_Event_Management_API.Controller
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReviewController : ControllerBase
    {
        private readonly IReviewRepository _reviewRepository;
        public ReviewController(IReviewRepository reviewRepository)
        {
            _reviewRepository = reviewRepository;
        }
        [Authorize]
        [HttpPost("AddReview")]
        public async Task<IActionResult> AddReview(ReviewDto reviewDto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null) return Unauthorized("User not authenticated.");

            var result = await _reviewRepository.AddReviewAsync(reviewDto,int.Parse(userId));
            if (result == "Success") 
                return Ok(new { Message = "Review added successfully." });
            return BadRequest(new {Message = result} );
        }
        [HttpGet("GetEventReviews/{eventId}")]
        public async Task<IActionResult> GetEventReviews(int eventId)
        {
            var reviews = await _reviewRepository.GetEventReviewsAsync(eventId);
            return Ok(reviews);
        }
    }
}