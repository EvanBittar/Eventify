using Moq;
using Xunit;
using Microsoft.AspNetCore.Mvc;
using Eventify_High_Performance_Event_Management_API.Controller;
using Eventify_High_Performance_Event_Management_API.Repository.Interfaces;
using Eventify_High_Performance_Event_Management_API.Models;
using Eventify_High_Performance_Event_Management_API.Dtos;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace Eventify.Tests
{
    public class EventControllerTests
    {
        private readonly Mock<IEventRepository> _mockRepo;
        private readonly EventController _controller;

        public EventControllerTests()
        {
            _mockRepo = new Mock<IEventRepository>();
            _controller = new EventController(_mockRepo.Object);
        }

        [Fact]
        public async Task GetAllEvents_ShouldReturnOk_WithPriceAndRating()
        {
            // Arrange
            var fakeEvents = new List<Event>
            {
                new Event{
                EventId = 1,
                Title = "Tech Talk",
                Price = 50.00m,
                AverageRating = 4.5m,
                ReviewsCount = 10 }
            };
            _mockRepo.Setup(repo => repo.GetAllEventsAsync()).ReturnsAsync(fakeEvents);

            // Act
            var result = await _controller.GetAllEvents();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedEvents = Assert.IsAssignableFrom<IEnumerable<Event>>(okResult.Value);
            var firstEvent = returnedEvents.First();

            Assert.Equal(50.00m,firstEvent.Price);
            Assert.Equal(4.5m,firstEvent.AverageRating);
            Assert.Equal(10,firstEvent.ReviewsCount);
        }

        [Fact]
        public async Task AddEvent_ShouldReturnOk_WhenSuccessful()
        {
            // Arrange
            var newEventDto = new EventToAddDto { Title = "New Event", MaxAttendees = 100 };
            _mockRepo.Setup(repo => repo.CreateEventAsync(newEventDto)).ReturnsAsync(true);

            // Act
            var result = await _controller.AddEvent(newEventDto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal("Event added successfully.", okResult.Value);
        }

        [Fact]
        public async Task UpdateEvent_ShouldReturnNotFound_WhenEventDoesNotExist()
        {
            // Arrange
            int eventId = 999;
            var updateDto = new EventToAddDto { Title = "Updated Title" };
            _mockRepo.Setup(repo => repo.UpdateEventAsync(eventId, updateDto)).ReturnsAsync(false);

            // Act
            var result = await _controller.UpdateEvent(eventId, updateDto);

            // Assert
            var notFoundResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Failed to Updated event.", notFoundResult.Value);
        }
        [Fact]
        public async Task SearchEvents_ShouldReturnFilteredResults()
        {
            // Arrange
            var searchResults = new List<dynamic>
            {
                new { EventId = 1, Title = "C# Workshop", NameCategory = "Programming", Price = 20.0m }
            };
            _mockRepo.Setup(repo => repo.SearchEvents("C#", null))
            .ReturnsAsync(searchResults);
            // Act
            var result = await _controller.SearchEvents("C#", null);
            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);
        }
        [Fact]
        public async Task SearchEvents_ShouldReturnEmpty_WhenNoMatch()
        {
            // Arrange
            string serachTerm = "none";
            var fakeEvent = new List<Event>();
            _mockRepo.Setup(repo => repo.SearchEvents(serachTerm, null)).ReturnsAsync(new List<dynamic>());
            // Act 
            var result = await _controller.SearchEvents(serachTerm, null);
            // Assert 
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedEvents = Assert.IsAssignableFrom<IEnumerable<dynamic>>(okResult.Value);
            Assert.Empty(returnedEvents);

        }
    }
}