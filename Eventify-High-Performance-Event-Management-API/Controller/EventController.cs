using Eventify_High_Performance_Event_Management_API.Dtos;
using Eventify_High_Performance_Event_Management_API.Models;
using Eventify_High_Performance_Event_Management_API.Repository.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace Eventify_High_Performance_Event_Management_API.Controller
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class EventController : ControllerBase
    {
        private readonly IEventRepository _eventRepository;
        private readonly ILogger<EventController> _logger;
        private readonly IMemoryCache _cache;

        private const string AllEventsCacheKey = "AllEvents";
        private const string DashboardStatsCacheKey = "DashboardStats";

        public EventController(IEventRepository eventRepository, ILogger<EventController> logger, IMemoryCache cache)
        {
            _eventRepository = eventRepository;
            _logger = logger;
            _cache = cache;
        }

        [AllowAnonymous]
        [HttpGet("GetAllEvents")]
        public async Task<IActionResult> GetAllEvents()
        {
            if (_cache.TryGetValue(AllEventsCacheKey, out IEnumerable<Event>? cachedEvents))
            {
                _logger.LogInformation("⚡ Returning {Count} events from Cache", cachedEvents!.Count());
                return Ok(cachedEvents);
            }
            _logger.LogInformation("📋 Cache miss - Fetching all events from Database");
            var events = await _eventRepository.GetAllEventsAsync();
            if(events != null)
            {
                _cache.Set(AllEventsCacheKey, events, TimeSpan.FromMinutes(5));
                _logger.LogInformation("✅ Cached {Count} events for 5 minutes", events.Count());
            }

            return Ok(events);
        }

        [AllowAnonymous]
        [HttpGet("GetAllEventsById/{id}")]
        public async Task<Event?> GetAllEventsById(int id)
        {
            string cacheKey = $"Event_{id}";

            // ✅ تحقق إذا الحدث موجود في الـ Cache
            if (_cache.TryGetValue(cacheKey, out Event? cachedEvent))
            {
                _logger.LogInformation("⚡ Returning Event {EventId} from Cache", id);
                return cachedEvent;
            }

            // ❌ ما في Cache — روح قاعدة البيانات
            _logger.LogInformation("🔍 Cache miss - Fetching Event {EventId} from Database", id);
            var eventItem = await _eventRepository.GetEventByIdAsync(id);

            if (eventItem != null)
            {
                // 💾 احفظ في الـ Cache لمدة 5 دقائق
                _cache.Set(cacheKey, eventItem, TimeSpan.FromMinutes(5));
                _logger.LogInformation("✅ Cached Event {EventId} for 5 minutes", id);
            }

            return eventItem;
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
                // 🗑️ امسح الـ Cache عشان يتحدث في المرة الجاية
                _cache.Remove(AllEventsCacheKey);
                _logger.LogInformation("✅ Event '{Title}' added - Cache cleared", eventToAddDto.Title);
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
                // 🗑️ امسح الـ Cache للحدث المحدد والقائمة الكاملة
                _cache.Remove($"Event_{id}");
                _cache.Remove(AllEventsCacheKey);
                _logger.LogInformation("✅ Event {EventId} updated - Cache cleared", id);
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
                // 🗑️ امسح الـ Cache
                _cache.Remove($"Event_{id}");
                _cache.Remove(AllEventsCacheKey);
                _logger.LogInformation("✅ Event {EventId} deleted - Cache cleared", id , currentUserId);
                return Ok("Event deleted successfully.");
            }

            _logger.LogError("❌ Failed to delete event {EventId}", id);
            return BadRequest("Failed to delete event.");
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("DashboardStats")]
        public async Task<IActionResult> GetDashboardStats()
        {
            // ✅ تحقق إذا الإحصائيات موجودة في الـ Cache
            if (_cache.TryGetValue(DashboardStatsCacheKey, out object? cachedStats))
            {
                _logger.LogInformation("⚡ Returning Dashboard Stats from Cache");
                return Ok(cachedStats);
            }

            _logger.LogInformation("📊 Cache miss - Fetching Dashboard Stats from Database");
            var stats = await _eventRepository.GetDashboardStatsAsync();

            // 💾 احفظ لمدة دقيقة وحدة فقط لأن الإحصائيات تتغير أكثر
            _cache.Set(DashboardStatsCacheKey, stats, TimeSpan.FromMinutes(1));
            _logger.LogInformation("✅ Cached Dashboard Stats for 1 minute");

            return Ok(stats);
        }
    }
}