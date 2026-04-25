using AutoMapper;
using Xunit;
using Moq;
using Microsoft.AspNetCore.Mvc;
using Eventify_High_Performance_Event_Management_API.Controller;
using Eventify_High_Performance_Event_Management_API.Models;
using Eventify_High_Performance_Event_Management_API.Dtos;
using Eventify_High_Performance_Event_Management_API.Repository.Interfaces;
using Eventify_High_Performance_Event_Management_API.Services;
using Eventify_High_Performance_Event_Management_API.Services.Interfaces;

namespace Eventify.Tests
{
    public class UserControllerTests
    {
        private readonly Mock<IUserRepository> _mockRepo;
        private readonly Mock<IAuthService> _mockAuthService;
        private readonly Mock<IMapper> _mockMapper;
        private readonly UserController _controller;

        public UserControllerTests()
        {
            _mockRepo = new Mock<IUserRepository>();
            _mockAuthService = new Mock<IAuthService>();
            _mockMapper = new Mock<IMapper>();

            _controller = new UserController(
                _mockRepo.Object,
                _mockAuthService.Object,
                _mockMapper.Object
            );
        }

        [Fact]
        public async Task GetUserById_ShouldReturnOk_WhenUserExists()
        {
            // Arrange
            var fakeUser = new User { UserId = 2, FirstName = "Evan" };
            _mockRepo.Setup(repo => repo.GetUserByIdAsync(It.IsAny<int>()))
                     .ReturnsAsync(fakeUser);

            // Act
            var result = await _controller.GetUserById(2);

            // Assert
            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task Login_ShouldReturnOk_WhenCredentialsAreValid()
        {
            // Arrange
            var email = "test@example.com";
            var password = "password123";
            var passwordHash = BCrypt.Net.BCrypt.HashPassword(password);

            var fakeUser = new User 
            { 
                UserId = 1, 
                Email = email, 
                PasswordHash = passwordHash, 
                IsAdmin = false 
            };

            _mockRepo.Setup(repo => repo.GetUserByEmailAsync(email))
                     .ReturnsAsync(fakeUser);

            _mockAuthService.Setup(s => s.VerifyPassword(password, passwordHash))
                            .Returns(true);

            _mockAuthService.Setup(s => s.CreateToken(fakeUser))
                            .Returns("fake.jwt.token");

            var loginDto = new UserToLoginDto { Email = email, Password = password };

            // Act
            var result = await _controller.Login(loginDto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);
        }

        [Fact]
        public async Task Login_ShouldReturnBadRequest_WhenPasswordIsIncorrect()
        {
            // Arrange
            var email = "test@test.com";
            var passwordHash = BCrypt.Net.BCrypt.HashPassword("correct_pass");

            var fakeUser = new User 
            { 
                Email = email, 
                PasswordHash = passwordHash 
            };

            _mockRepo.Setup(repo => repo.GetUserByEmailAsync(email))
                     .ReturnsAsync(fakeUser);

            _mockAuthService.Setup(s => s.VerifyPassword("wrong_password", passwordHash))
                            .Returns(false);

            var loginDto = new UserToLoginDto { Email = email, Password = "wrong_password" };

            // Act
            var result = await _controller.Login(loginDto);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }
    }
}