namespace Eventify_High_Performance_Event_Management_API.Repository.Interfaces
{
    public interface IEmailService
    {
        Task SendEmailAsync(string toEmail, string subject, string body);
    }
}