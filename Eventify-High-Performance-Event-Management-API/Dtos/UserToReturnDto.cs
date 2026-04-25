namespace Eventify_High_Performance_Event_Management_API.Dtos
{
    public class UserToReturnDto
    {
        public int UserId { get; set; }
        public string FirstName { get; set; } = "";
        public string LastName { get; set; } = "";
        public string Email { get; set; } = "";
        public bool IsVerified { get; set; }
        public bool IsAdmin { get; set; }
    }
}