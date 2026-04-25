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
    public class ReviewControllerTests
    {
        private readonly Mock<IEventRepository> _mockEventRepo;
        private readonly EventController _eventController;

        public ReviewControllerTests()
        {
            _mockEventRepo = new Mock<IEventRepository>();
            _eventController = new EventController(_mockEventRepo.Object);
        }
        [Fact]
        public async Task SearchEvents_ShouldReturnFilteredResults()
        {
            // Arrange
            var searchResults = new List<dynamic>
            {
                new { EventId = 1, Title = "C# Workshop", NameCategory = "Programming", Price = 20.0m }
            };

            _mockEventRepo.Setup(repo => repo.SearchEvents("C#", null))
                          .ReturnsAsync(searchResults);

            // Act
            var result = await _eventController.SearchEvents("C#", null);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);
        }
    }
}