using System.Security.Claims;
using Eventify_High_Performance_Event_Management_API.Dtos;
using Eventify_High_Performance_Event_Management_API.Repository.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Eventify_High_Performance_Event_Management_API.Controller
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class BookingController : ControllerBase
    {
        private readonly IBookingRepository _bookingRepository;
        private readonly IEmailService _emailService;
        private readonly IUserRepository _userRepo;
        public BookingController(IBookingRepository bookingRepository, IEmailService emailService, IUserRepository userRepo )
        {
            _bookingRepository = bookingRepository;
            _emailService = emailService;
            _userRepo = userRepo;
        }

        [HttpDelete("CancelBooking/{bookingId}")]
        public async Task<IActionResult> CancelBooking(int bookingId)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null) return Unauthorized("User not authenticated.");

            var success = await _bookingRepository.CancelBookingAsync(bookingId);

            if (success)
                return Ok(new { Message = "Booking cancelled successfully." });

            return BadRequest(new { Message = "Could not cancel booking or booking not found." });
        }
        [HttpGet("GetMyTickets")]
        public async Task<IActionResult> GetMyTickets()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null) return Unauthorized("User not authenticated.");

            var bookings = await _bookingRepository.GetUserBookingsAsync(int.Parse(userIdClaim));
            return Ok(bookings);
        }
        [HttpPost("CreateBooking")]
        [Authorize]
        public async Task<IActionResult> CreateBooking(int eventId)
        {
            var UserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (UserId == null) return Unauthorized("User not authenticated.");

            var booking = new BookingDto
            {
                EventId = eventId,
                UserId = int.Parse(UserId)
            };
            int userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            var user = await _userRepo.GetUserByIdAsync(userId);

            if (user == null || !user.IsVerified)
            {
                return BadRequest("Your email is not verified. Please verify your email to book events.");
            }
            var result = await _bookingRepository.CreateBookingAsync(booking);
            if (result == "Success")
            {

                var userEmail = User.FindFirst(ClaimTypes.Email)?.Value;

                if (!string.IsNullOrEmpty(userEmail))
                {
                    _ = _emailService.SendEmailAsync(userEmail,
                        "Booking Confirmed! 🎟️",
                        $"<h1>Success!</h1><p>Your booking for event ID {booking.EventId} is confirmed. See you there!</p>");
                }

                return Ok(new { Message = "Booking successful and email sent." });
            }

            return BadRequest(new { Message = result });
        }
    }
}