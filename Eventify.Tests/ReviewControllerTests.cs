using System.Security.Claims;
using Moq;
using Xunit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Eventify_High_Performance_Event_Management_API.Controller;
using Eventify_High_Performance_Event_Management_API.Repository.Interfaces;
using Eventify_High_Performance_Event_Management_API.Dtos;

namespace Eventify.Tests
{
    public class ReviewControllerTests
    {
        private readonly Mock<IReviewRepository> _mockRepo;
        private readonly ReviewController _controller;

        public ReviewControllerTests()
        {
            _mockRepo = new Mock<IReviewRepository>();
            _controller = new ReviewController(_mockRepo.Object);

            // محاكاة المستخدم المسجل
            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.NameIdentifier, "1")
            }, "mock"));

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };
        }

        // ✅ Test 1: إضافة review بنجاح
        [Fact]
        public async Task AddReview_ShouldReturnOk_WhenSuccessful()
        {
            // Arrange
            var reviewDto = new ReviewDto { EventId = 1, Rating = 5, Comment = "Great event!" };

            _mockRepo.Setup(repo => repo.AddReviewAsync(reviewDto, 1))
                     .ReturnsAsync("Success");

            // Act
            var result = await _controller.AddReview(reviewDto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            dynamic data = okResult.Value!;
            string message = data.GetType().GetProperty("Message").GetValue(data, null);
            Assert.Equal("Review added successfully.", message);
        }

        // ❌ Test 2: إضافة review فاشلة (مثلاً راجع مرتين)
        [Fact]
        public async Task AddReview_ShouldReturnBadRequest_WhenFails()
        {
            // Arrange
            var reviewDto = new ReviewDto { EventId = 1, Rating = 3, Comment = "OK" };

            _mockRepo.Setup(repo => repo.AddReviewAsync(reviewDto, 1))
                     .ReturnsAsync("You have already reviewed this event.");

            // Act
            var result = await _controller.AddReview(reviewDto);

            // Assert
            var badResult = Assert.IsType<BadRequestObjectResult>(result);
            dynamic data = badResult.Value!;
            string message = data.GetType().GetProperty("Message").GetValue(data, null);
            Assert.Equal("You have already reviewed this event.", message);
        }

        // ❌ Test 3: مستخدم غير مسجل يحاول يضيف review
        [Fact]
        public async Task AddReview_ShouldReturnUnauthorized_WhenUserNotAuthenticated()
        {
            // Arrange — مستخدم بدون claims
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal() }
            };

            var reviewDto = new ReviewDto { EventId = 1, Rating = 4, Comment = "Nice" };

            // Act
            var result = await _controller.AddReview(reviewDto);

            // Assert
            Assert.IsType<UnauthorizedObjectResult>(result);
        }

        // ✅ Test 4: جلب reviews بنجاح
        [Fact]
        public async Task GetEventReviews_ShouldReturnOk_WithReviews()
        {
            // Arrange
            int eventId = 1;
            var fakeReviews = new List<dynamic>
            {
                new { ReviewId = 1, Rating = 5, Comment = "Amazing!", UserName = "Evan" },
                new { ReviewId = 2, Rating = 4, Comment = "Good event", UserName = "John" }
            };

            _mockRepo.Setup(repo => repo.GetEventReviewsAsync(eventId))
                     .ReturnsAsync(fakeReviews);

            // Act
            var result = await _controller.GetEventReviews(eventId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);
        }

        // ✅ Test 5: جلب reviews لحدث بدون reviews
        [Fact]
        public async Task GetEventReviews_ShouldReturnOk_WithEmptyList()
        {
            // Arrange
            int eventId = 999;
            _mockRepo.Setup(repo => repo.GetEventReviewsAsync(eventId))
                     .ReturnsAsync(new List<dynamic>());

            // Act
            var result = await _controller.GetEventReviews(eventId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var reviews = Assert.IsAssignableFrom<IEnumerable<dynamic>>(okResult.Value);
            Assert.Empty(reviews);
        }
    }
}