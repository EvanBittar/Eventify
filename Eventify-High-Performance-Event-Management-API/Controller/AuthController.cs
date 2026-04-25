using Eventify_High_Performance_Event_Management_API.Dtos;
using Eventify_High_Performance_Event_Management_API.Repository.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Eventify_High_Performance_Event_Management_API.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthRepository _authRepo;
        private readonly IEmailService _emailService;
        private readonly IUserRepository _userRepo;
        private static readonly Random _random  = new();
        public AuthController(IAuthRepository authRepo, IEmailService emailService, IUserRepository userRepo)
        {
            _authRepo = authRepo;
            _emailService = emailService;
            _userRepo = userRepo;
        }
        [HttpPost("ForgetPassword")]
        public async Task<IActionResult> ForgetPassword(string email)
        {
            var user = await _userRepo.GetUserByEmailAsync(email);
            if (user == null) return NotFound("User not found");

            string code = _random.Next(100000, 999999).ToString();
            var expiration = DateTime.Now.AddMinutes(15);

            var saved = await _authRepo.SaveVerificationCode(email, code, expiration);
            if (saved)
            {
                await _emailService.SendEmailAsync(email, "Reset Code", $"Your code is: {code}");
                return Ok("Verification code sent to your email.");
            }
            return BadRequest("Could not process request.");
        }
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword(ResetPasswordDto resetPasswordDto)
        {
            var user = await _authRepo.GetUserByResetCode(resetPasswordDto.Email, resetPasswordDto.Code);
            if (user == null) return BadRequest("Invalid or expired code.");

            string passwordHash = BCrypt.Net.BCrypt.HashPassword(resetPasswordDto.NewPassword);

            var updated = await _authRepo.UpdatePassword(resetPasswordDto.Email, passwordHash);
            if (updated) return Ok("Password updated successfully.");

            return BadRequest("Error updating password.");
        }
        [HttpPost("verify-email")]
        public async Task<IActionResult> VerifyEmail(string email, string code)
        {
            var user = await _authRepo.GetUserByResetCode(email, code);
            if (user == null) return BadRequest("Invalid or expired verification code.");

            var verified = await _authRepo.VerifyUserEmail(email);
            if (verified) return Ok("Email verified successfully.");

            return BadRequest("Verification failed.");
        }
    }
}