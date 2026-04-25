using System.ComponentModel.DataAnnotations;

namespace Eventify_High_Performance_Event_Management_API.Dtos
{  
public class UserToAddDto
    {
        [Required(ErrorMessage = "First name is required.")]
        public string FirstName { get; set; } = string.Empty;
        [Required]
        public string LastName { get; set; } = string.Empty;
        [Required]
        [EmailAddress(ErrorMessage = "Invalid Email Address")]
        public string Email { get; set; } = string.Empty;
        [Required]
        [MinLength(8, ErrorMessage = "Password must be at least 8 characters long.")]
        public string PasswordHash { get; set; } = string.Empty;
        [Required]
        public bool IsAdmin { get; set; }
    }
}