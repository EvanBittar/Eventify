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
        public EventController(IEventRepository eventRepository)
        {
            _eventRepository = eventRepository;
        }
        [AllowAnonymous]
        [HttpGet("GetAllEvents")]
        public async Task<IActionResult> GetAllEvents()
        {
            var events = await _eventRepository.GetAllEventsAsync();
            return Ok(events);
        }
        [AllowAnonymous]
        [HttpGet("GetAllEventsById/{id}")]
        public async Task<Event?> GetAllEventsById(int id)
        {
            return await _eventRepository.GetEventByIdAsync(id);
        }
        [AllowAnonymous]
        [HttpGet("SearchEvents")]
        public async Task<IActionResult> SearchEvents(string? title = null, int? CategoryId = null)
        {
            var events = await _eventRepository.SearchEvents(title, CategoryId);
            return Ok(events);
        }
        [HttpPost("AddEvent")]
        [Authorize(Roles = "Admin,Organizer")]
        public async Task<IActionResult> AddEvent(EventToAddDto eventToAddDto)
        {
            if (await _eventRepository.CreateEventAsync(eventToAddDto)) return Ok("Event added successfully.");
            return BadRequest("Failed to add event.");
        }
        [HttpPut("UpdateEvent/{id}")]
        [Authorize(Roles = "Admin,Organizer")]
        public async Task<IActionResult> UpdateEvent(int id, EventToAddDto eventToAddDto)
        {
            if (await _eventRepository.UpdateEventAsync(id, eventToAddDto)) return Ok("Event Updated successfully.");
            return BadRequest("Failed to Updated event.");
        }
        [HttpDelete("DeleteEvent/{id}")]
        // [Authorize]
        [Authorize(Roles = "Admin,Organizer")]
        public async Task<IActionResult> DeleteEvent(int id)
        {
            var existingEvent = await _eventRepository.GetEventByIdAsync(id);
            if (existingEvent == null) return NotFound("Event not found.");

            var currentUserId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value!);
            var userRole = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;

            if (userRole != "Admin" && existingEvent.CreatedBy != currentUserId)
            {
                return Forbid("You are not authorized to delete this event.");
            }

            if (await _eventRepository.DeleteEventAsync(id))
                return Ok("Event deleted successfully.");

            return BadRequest("Failed to delete event.");
        }
        [Authorize(Roles = "Admin")]
        [HttpGet("DashboardStats")]
        public async Task<IActionResult> GetDashboardStats()
        {
            var stats = await _eventRepository.GetDashboardStatsAsync();
            return Ok(stats);
        }
    }
}