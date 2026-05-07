using System.Security.Claims;
using Eventify_High_Performance_Event_Management_API.Dtos;
using Eventify_High_Performance_Event_Management_API.Repository.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Serilog;

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
        private readonly ILogger<BookingController> _logger;

        public BookingController(IBookingRepository bookingRepository, IEmailService emailService, IUserRepository userRepo, ILogger<BookingController> logger)
        {
            _bookingRepository = bookingRepository;
            _emailService = emailService;
            _userRepo = userRepo;
            _logger = logger;
        }

        [HttpDelete("CancelBooking/{bookingId}")]
        public async Task<IActionResult> CancelBooking(int bookingId)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null)
            {
                _logger.LogWarning("⚠️ CancelBooking failed - User not authenticated");
                return Unauthorized("User not authenticated.");
            }

            _logger.LogInformation("🎫 User {UserId} attempting to cancel booking {BookingId}", userIdClaim, bookingId);

            var success = await _bookingRepository.CancelBookingAsync(bookingId);

            if (success)
            {
                _logger.LogInformation("✅ Booking {BookingId} cancelled successfully by User {UserId}", bookingId, userIdClaim);
                return Ok(new { Message = "Booking cancelled successfully." });
            }

            _logger.LogWarning("❌ Booking {BookingId} could not be cancelled - not found or already cancelled", bookingId);
            return BadRequest(new { Message = "Could not cancel booking or booking not found." });
        }

        [HttpGet("GetMyTickets")]
        public async Task<IActionResult> GetMyTickets()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null)
            {
                _logger.LogWarning("⚠️ GetMyTickets failed - User not authenticated");
                return Unauthorized("User not authenticated.");
            }

            _logger.LogInformation("🎟️ User {UserId} fetching their tickets", userIdClaim);

            var bookings = await _bookingRepository.GetUserBookingsAsync(int.Parse(userIdClaim));

            _logger.LogInformation("✅ Returned {Count} tickets for User {UserId}", bookings.Count(), userIdClaim);
            return Ok(bookings);
        }

        [HttpPost("CreateBooking")]
        public async Task<IActionResult> CreateBooking(int eventId)
        {
            var UserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (UserId == null)
            {
                _logger.LogWarning("⚠️ CreateBooking failed - User not authenticated");
                return Unauthorized("User not authenticated.");
            }

            _logger.LogInformation("📅 User {UserId} attempting to book Event {EventId}", UserId, eventId);

            var booking = new BookingDto
            {
                EventId = eventId,
                UserId = int.Parse(UserId)
            };

            var user = await _userRepo.GetUserByIdAsync(int.Parse(UserId));

            if (user == null || !user.IsVerified)
            {
                _logger.LogWarning("⚠️ User {UserId} email not verified - booking rejected", UserId);
                return BadRequest("Your email is not verified. Please verify your email to book events.");
            }

            var result = await _bookingRepository.CreateBookingAsync(booking);

            if (result == "Success")
            {
                _logger.LogInformation("✅ Booking created successfully for User {UserId} on Event {EventId}", UserId, eventId);

                var userEmail = User.FindFirst(ClaimTypes.Email)?.Value;
                if (!string.IsNullOrEmpty(userEmail))
                {
                    _ = _emailService.SendEmailAsync(userEmail,
                        "Booking Confirmed! 🎟️",
                        $"<h1>Success!</h1><p>Your booking for event ID {booking.EventId} is confirmed. See you there!</p>");

                    _logger.LogInformation("📧 Confirmation email sent to {Email}", userEmail);
                }

                return Ok(new { Message = "Booking successful and email sent." });
            }

            _logger.LogWarning("❌ Booking failed for User {UserId} on Event {EventId} - Reason: {Reason}", UserId, eventId, result);
            return BadRequest(new { Message = result });
        }
    }
}