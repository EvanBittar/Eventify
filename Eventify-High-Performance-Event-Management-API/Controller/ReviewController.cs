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
        private readonly ILogger<ReviewController> _logger;

        public ReviewController(IReviewRepository reviewRepository, ILogger<ReviewController> logger)
        {
            _reviewRepository = reviewRepository;
            _logger = logger;
        }

        [Authorize]
        [HttpPost("AddReview")]
        public async Task<IActionResult> AddReview(ReviewDto reviewDto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
            {
                _logger.LogWarning("⚠️ AddReview failed - User not authenticated");
                return Unauthorized("User not authenticated.");
            }

            _logger.LogInformation("⭐ User {UserId} adding review for Event {EventId}", userId, reviewDto.EventId);

            var result = await _reviewRepository.AddReviewAsync(reviewDto, int.Parse(userId));

            if (result == "Success")
            {
                _logger.LogInformation("✅ Review added successfully by User {UserId} for Event {EventId}", userId, reviewDto.EventId);
                return Ok(new { Message = "Review added successfully." });
            }

            _logger.LogWarning("❌ Review failed for User {UserId} - Reason: {Reason}", userId, result);
            return BadRequest(new { Message = result });
        }

        [HttpGet("GetEventReviews/{eventId}")]
        public async Task<IActionResult> GetEventReviews(int eventId)
        {
            _logger.LogInformation("📋 Fetching reviews for Event {EventId}", eventId);
            var reviews = await _reviewRepository.GetEventReviewsAsync(eventId);
            return Ok(reviews);
        }
    }
}