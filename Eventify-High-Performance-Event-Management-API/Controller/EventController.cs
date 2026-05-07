using Eventify_High_Performance_Event_Management_API.Dtos;
using Eventify_High_Performance_Event_Management_API.Models;
using Eventify_High_Performance_Event_Management_API.Repository.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Eventify_High_Performance_Event_Management_API.Controller
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class EventController : ControllerBase
    {
        private readonly IEventRepository _eventRepository;
        private readonly ILogger<EventController> _logger;

        public EventController(IEventRepository eventRepository, ILogger<EventController> logger)
        {
            _eventRepository = eventRepository;
            _logger = logger;
        }

        [AllowAnonymous]
        [HttpGet("GetAllEvents")]
        public async Task<IActionResult> GetAllEvents()
        {
            _logger.LogInformation("📋 Fetching all events");
            var events = await _eventRepository.GetAllEventsAsync();
            _logger.LogInformation("✅ Returned {Count} events", events.Count());
            return Ok(events);
        }

        [AllowAnonymous]
        [HttpGet("GetAllEventsById/{id}")]
        public async Task<Event?> GetAllEventsById(int id)
        {
            _logger.LogInformation("🔍 Fetching event {EventId}", id);
            return await _eventRepository.GetEventByIdAsync(id);
        }

        [AllowAnonymous]
        [HttpGet("SearchEvents")]
        public async Task<IActionResult> SearchEvents(string? title = null, int? CategoryId = null)
        {
            _logger.LogInformation("🔎 Searching events - Title: {Title}, CategoryId: {CategoryId}", title, CategoryId);
            var events = await _eventRepository.SearchEvents(title, CategoryId);
            return Ok(events);
        }

        [HttpPost("AddEvent")]
        [Authorize(Roles = "Admin,Organizer")]
        public async Task<IActionResult> AddEvent(EventToAddDto eventToAddDto)
        {
            _logger.LogInformation("➕ Adding new event: {Title}", eventToAddDto.Title);

            if (await _eventRepository.CreateEventAsync(eventToAddDto))
            {
                _logger.LogInformation("✅ Event '{Title}' added successfully", eventToAddDto.Title);
                return Ok("Event added successfully.");
            }

            _logger.LogWarning("❌ Failed to add event: {Title}", eventToAddDto.Title);
            return BadRequest("Failed to add event.");
        }

        [HttpPut("UpdateEvent/{id}")]
        [Authorize(Roles = "Admin,Organizer")]
        public async Task<IActionResult> UpdateEvent(int id, EventToAddDto eventToAddDto)
        {
            _logger.LogInformation("✏️ Updating event {EventId}", id);

            if (await _eventRepository.UpdateEventAsync(id, eventToAddDto))
            {
                _logger.LogInformation("✅ Event {EventId} updated successfully", id);
                return Ok("Event Updated successfully.");
            }

            _logger.LogWarning("❌ Failed to update event {EventId}", id);
            return BadRequest("Failed to Updated event.");
        }

        [HttpDelete("DeleteEvent/{id}")]
        [Authorize(Roles = "Admin,Organizer")]
        public async Task<IActionResult> DeleteEvent(int id)
        {
            _logger.LogInformation("🗑️ Delete requested for event {EventId}", id);

            var existingEvent = await _eventRepository.GetEventByIdAsync(id);
            if (existingEvent == null)
            {
                _logger.LogWarning("⚠️ Event {EventId} not found for deletion", id);
                return NotFound("Event not found.");
            }

            var currentUserId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value!);
            var userRole = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;

            if (userRole != "Admin" && existingEvent.CreatedBy != currentUserId)
            {
                _logger.LogWarning("🚫 User {UserId} not authorized to delete event {EventId}", currentUserId, id);
                return Forbid("You are not authorized to delete this event.");
            }

            if (await _eventRepository.DeleteEventAsync(id))
            {
                _logger.LogInformation("✅ Event {EventId} deleted successfully by User {UserId}", id, currentUserId);
                return Ok("Event deleted successfully.");
            }

            _logger.LogError("❌ Failed to delete event {EventId}", id);
            return BadRequest("Failed to delete event.");
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("DashboardStats")]
        public async Task<IActionResult> GetDashboardStats()
        {
            _logger.LogInformation("📊 Admin fetching dashboard stats");
            var stats = await _eventRepository.GetDashboardStatsAsync();
            return Ok(stats);
        }
    }
}