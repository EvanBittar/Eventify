using System.Security.Claims;

namespace Eventify_High_Performance_Event_Management_API.Middlewares
{
    public class EmailVerificationMiddleware
    {
        private readonly RequestDelegate _next;
        public EmailVerificationMiddleware(RequestDelegate next)
        {
            _next = next;
        }
        public async Task InvokeAsync(HttpContext context)
        {
            if (context.User.Identity?.IsAuthenticated == true)
            {
                var isVerifiedClaim = context.User.FindFirst("IsVerified")?.Value;
                var path = context.Request.Path.Value?.ToLower();

                if (isVerifiedClaim != "True" && !path!.Contains("verify-email") && !path.Contains("login"))
                {
                    context.Response.StatusCode = StatusCodes.Status403Forbidden;
                    await context.Response.WriteAsync("Account not verified. Please verify your email to access this feature.");
                    return;
                }
            }
            await _next(context);
        }
    }
}