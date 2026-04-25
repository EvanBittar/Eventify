using System.Security.Claims;
using Eventify_High_Performance_Event_Management_API.Controller;
using Eventify_High_Performance_Event_Management_API.Dtos;
using Eventify_High_Performance_Event_Management_API.Models;
using Eventify_High_Performance_Event_Management_API.Repository.Interfaces;
using Eventify_High_Performance_Event_Management_API.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace Eventify.Tests
{
    public class BookingControllerTests
    {
        private readonly Mock<IBookingRepository> _mockRepo;
        private readonly Mock<IEmailService> _mockEmailService;
        private readonly Mock<IUserRepository> _mockUserRepo;
        private readonly BookingController _controller;

        public BookingControllerTests()
        {
            _mockRepo = new Mock<IBookingRepository>();
            _mockEmailService = new Mock<IEmailService>();
            _mockUserRepo = new Mock<IUserRepository>();

            _controller = new BookingController(
                _mockRepo.Object,
                _mockEmailService.Object,
                _mockUserRepo.Object
            );

            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.NameIdentifier, "1")
            }, "mock"));

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };
        }

        [Fact]
        public async Task CreateBooking_ShouldReturnOk_WhenSuccessful()
        {
            int eventId = 3;
            int userId = 1;

            var fakeUser = new User { UserId = userId, IsVerified = true };

            _mockRepo.Setup(repo => repo.CreateBookingAsync(It.IsAny<BookingDto>()))
                     .ReturnsAsync("Success");

            _mockUserRepo.Setup(repo => repo.GetUserByIdAsync(userId))
                         .ReturnsAsync(fakeUser);

            var result = await _controller.CreateBooking(eventId);

            var okResult = Assert.IsType<OkObjectResult>(result);
            dynamic data = okResult.Value!;
            string message = data.GetType().GetProperty("Message").GetValue(data, null);
            Assert.Equal("Booking successful and email sent.", message);
        }
        [Fact]
        public async Task CreateBooking_ShouldReturnBadRequest_WhenEventIsFull()
        {
            int eventId = 3;
            _mockRepo.Setup(repo => repo.CreateBookingAsync(It.IsAny<BookingDto>()))
                     .ReturnsAsync("Event is full");

            var result = await _controller.CreateBooking(eventId);

            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task CancelBooking_ShouldReturnOk_WhenSuccessful()
        {
            int bookingid = 3;
            _mockRepo.Setup(repo => repo.CancelBookingAsync(bookingid))
                     .ReturnsAsync(true);

            var result = await _controller.CancelBooking(bookingid);

            var okResult = Assert.IsType<OkObjectResult>(result);
            dynamic data = okResult.Value;
            string message = data.GetType().GetProperty("Message").GetValue(data, null);
            Assert.Equal("Booking cancelled successfully.", message);
        }
        [Fact]
        public async Task CreateBooking_ShouldReturnBadRequest_WhenUserNotVerified()
        {
            int eventId = 3;
            int userId = 1;

            var fakeUser = new User { UserId = userId, IsVerified = false };

            _mockUserRepo.Setup(repo => repo.GetUserByIdAsync(userId))
                         .ReturnsAsync(fakeUser);

            var result = await _controller.CreateBooking(eventId);

            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task CancelBooking_ShouldReturnBadRequest_WhenFails()
        {
            int bookingid = 3;
            _mockRepo.Setup(repo => repo.CancelBookingAsync(bookingid))
                     .ReturnsAsync(false);

            var result = await _controller.CancelBooking(bookingid);

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            dynamic data = badRequestResult.Value;
            string message = data.GetType().GetProperty("Message").GetValue(data, null);
            Assert.Equal("Could not cancel booking or booking not found.", message);
        }

        [Fact]
        public async Task GetAllTickets_ShouldReturnOk_WithListOfTickets()
        {
            int userId = 1;
            var bookinglist = new List<dynamic>
            {
                new { BookingId = 101, Title = "Tech Conference", StartDate = DateTime.Now.AddDays(5),
                      Location = "Damascus", NameCategory = "IT", Status = 1 },
                new { BookingId = 102, Title = "Music Fest", StartDate = DateTime.Now.AddDays(10),
                      Location = "Aleppo", NameCategory = "Art", Status = 1 }
            };
            _mockRepo.Setup(repo => repo.GetUserBookingsAsync(userId)).ReturnsAsync(bookinglist);

            var result = await _controller.GetMyTickets();

            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedTickets = Assert.IsAssignableFrom<IEnumerable<dynamic>>(okResult.Value);
            Assert.Equal(2, returnedTickets.Count());
        }
    }
}