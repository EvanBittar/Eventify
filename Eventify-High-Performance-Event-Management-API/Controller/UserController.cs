using Eventify_High_Performance_Event_Management_API.Dtos;
using Eventify_High_Performance_Event_Management_API.Models;
using Microsoft.AspNetCore.Mvc;
using Eventify_High_Performance_Event_Management_API.Repository.Interfaces;
using Eventify_High_Performance_Event_Management_API.Services.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.RateLimiting;

namespace Eventify_High_Performance_Event_Management_API.Controller
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly IMapper _mapper;
        private readonly IUserRepository _userRepository;
        private readonly IAuthService _authService;
        private readonly ILogger<UserController> _logger;

        public UserController(IUserRepository userRepository, IAuthService authService, IMapper mapper, ILogger<UserController> logger)
        {
            _mapper = mapper;
            _userRepository = userRepository;
            _authService = authService;
            _logger = logger;
        }

        [HttpGet("GetUsers")]
        public async Task<IActionResult> GetUsers()
        {
            _logger.LogInformation("📋 Fetching all users");
            var users = await _userRepository.GetAllUsersAsync();
            var usersToReturn = _mapper.Map<IEnumerable<UserToReturnDto>>(users);
            _logger.LogInformation("✅ Returned {Count} users", usersToReturn.Count());
            return Ok(usersToReturn);
        }

        [HttpGet("GetUserById/{id}")]
        public async Task<IActionResult> GetUserById(int id)
        {
            _logger.LogInformation("🔍 Fetching user {UserId}", id);
            var user = await _userRepository.GetUserByIdAsync(id);
            if (user == null)
            {
                _logger.LogWarning("⚠️ User {UserId} not found", id);
                return NotFound("User not found");
            }
            return Ok(_mapper.Map<UserToReturnDto>(user));
        }

        [HttpPost("GetUserByEmailAsync")]
        public async Task<IActionResult> GetUserByEmailAsync(string email)
        {
            _logger.LogInformation("🔍 Fetching user by email {Email}", email);
            var user = await _userRepository.GetUserByEmailAsync(email);
            if (user == null)
            {
                _logger.LogWarning("⚠️ User with email {Email} not found", email);
                return NotFound("User not found");
            }
            return Ok(user);
        }

        [HttpPost("Register")]
        public async Task<IActionResult> Register(UserToAddDto userToAddDto)
        {
            _logger.LogInformation("📝 Registration attempt for email {Email}", userToAddDto.Email);

            if (await _userRepository.GetUserByEmailAsync(userToAddDto.Email) != null)
            {
                _logger.LogWarning("⚠️ Registration failed - Email {Email} already exists", userToAddDto.Email);
                return BadRequest("Email already exists");
            }

            var user = _mapper.Map<User>(userToAddDto);
            user.PasswordHash = _authService.HashPassword(userToAddDto.PasswordHash);
            user.IsVerified = false;

            if (await _userRepository.AddUserAsync(user))
            {
                _logger.LogInformation("✅ User registered successfully with email {Email}", userToAddDto.Email);
                return Ok("User registered successfully");
            }

            _logger.LogError("❌ Failed to register user with email {Email}", userToAddDto.Email);
            return BadRequest("Failed to register user");
        }

        [HttpPost("Login")]
        [EnableRateLimiting("LoginPolicy")]
        public async Task<IActionResult> Login(UserToLoginDto userLogin)
        {
            _logger.LogInformation("🔐 Login attempt for email {Email}", userLogin.Email);

            var user = await _userRepository.GetUserByEmailAsync(userLogin.Email);

            if (user == null || !_authService.VerifyPassword(userLogin.Password, user.PasswordHash))
            {
                _logger.LogWarning("❌ Login failed for email {Email} - Invalid credentials", userLogin.Email);
                return BadRequest("Invalid email or password");
            }

            _logger.LogInformation("✅ User {Email} logged in successfully", userLogin.Email);
            return Ok(new { token = _authService.CreateToken(user) });
        }
    }
}