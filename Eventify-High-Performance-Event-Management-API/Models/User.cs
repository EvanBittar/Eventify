namespace Eventify_High_Performance_Event_Management_API.Models
{
    public class User
    {
        public int UserId { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public bool IsAdmin { get; set; } = false ;
        public bool IsVerified { get; set; }
        public string? VerificationCode { get; set; }
        public string Roles { get; set; } = "User";

        public DateTime? CodeExpiration { get; set; }
    }
}