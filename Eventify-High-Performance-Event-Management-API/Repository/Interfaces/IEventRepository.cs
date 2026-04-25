using Eventify_High_Performance_Event_Management_API.Models;
using Eventify_High_Performance_Event_Management_API.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace Eventify_High_Performance_Event_Management_API.Repository.Interfaces
{
    public interface IEventRepository
    {
        Task<Event?> GetEventByIdAsync(int id);
        Task<IEnumerable<dynamic>> GetAllEventsAsync();
        Task<bool> CreateEventAsync(EventToAddDto eventToAddDto);
        Task<IEnumerable<dynamic>> SearchEvents(string? title = null , int? CategoryId = null);
        Task<bool> UpdateEventAsync(int id, EventToAddDto eventToAddDto);
        Task<DashboardStatsDto?> GetDashboardStatsAsync();
        Task<bool> DeleteEventAsync(int id);
    }
}