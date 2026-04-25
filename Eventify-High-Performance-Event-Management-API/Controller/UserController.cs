using Eventify_High_Performance_Event_Management_API.Dtos;
using Eventify_High_Performance_Event_Management_API.Models;
using Microsoft.AspNetCore.Mvc;
using Eventify_High_Performance_Event_Management_API.Repository.Interfaces;
using Eventify_High_Performance_Event_Management_API.Services.Interfaces;
using AutoMapper;

namespace Eventify_High_Performance_Event_Management_API.Controller
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly IMapper _mapper;
        private readonly IUserRepository _userRepository;
        private readonly IAuthService _authService;
        public UserController(IUserRepository userRepository, IAuthService authService, IMapper mapper)
        {
            _mapper = mapper;
            _userRepository = userRepository;
            _authService = authService;
        }
        // [HttpGet("test")]
        // public async Task<IActionResult> Test()
        // {
        //     return Ok(await _dapper.LoadData<DateTime>("SELECT GETDATE()"));
        // }

        [HttpGet("GetUsers")]
        public async Task<IActionResult> GetUsers()
        {
            var users = await _userRepository.GetAllUsersAsync();

            var usersToReturn = _mapper.Map<IEnumerable<UserToReturnDto>>(users);

            return Ok(usersToReturn);
        }
        [HttpGet("GetUserById/{id}")]
        public async Task<IActionResult> GetUserById(int id)
        {
            var user = await _userRepository.GetUserByIdAsync(id);
            if (user == null) return NotFound("User not found");

            return Ok(_mapper.Map<UserToReturnDto>(user));
        }
        [HttpPost("GetUserByEmailAsync")]
        public async Task<IActionResult> GetUserByEmailAsync(string email)
        {
            var user = await _userRepository.GetUserByEmailAsync(email);
            if (user == null) return NotFound("User not found");
            return Ok(user);
        }

        [HttpPost("Register")]
        public async Task<IActionResult> Register(UserToAddDto userToAddDto)
        {
            if (await _userRepository.GetUserByEmailAsync(userToAddDto.Email) != null)
                return BadRequest("Email already exists");

            var user = _mapper.Map<User>(userToAddDto);

            user.PasswordHash = _authService.HashPassword(userToAddDto.PasswordHash);
            user.IsVerified = false;

            if (await _userRepository.AddUserAsync(user))
                return Ok("User registered successfully");

            return BadRequest("Failed to register user");
            // return StatusCode(500, "Something went wrong while saving the user.");
            // return await _userRepository.GetUserByEmailAsync(userToAddDto.Email) != null ? Ok("User registered successfully") : BadRequest("Failed to register user");
        }
        [HttpPost("Login")]
        public async Task<IActionResult> Login(UserToLoginDto userLogin)
        {
            var user = await _userRepository.GetUserByEmailAsync(userLogin.Email);

            if (user == null || !_authService.VerifyPassword(userLogin.Password, user.PasswordHash))
            {
                return BadRequest("Invalid email or password");
            }

            return Ok(new { token = _authService.CreateToken(user) });
        }
    }
}