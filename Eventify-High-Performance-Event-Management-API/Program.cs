using System.Threading.RateLimiting;
using Eventify_High_Performance_Event_Management_API.Helpers;
using Eventify_High_Performance_Event_Management_API.Middlewares;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.OpenApi;
using Serilog;
using Swashbuckle.AspNetCore.Filters;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console()
    .WriteTo.File("Logs/eventify.log", rollingInterval: RollingInterval.Day)
    .CreateLogger();
var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog();
builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("GeneralPolicy", opt =>
    {
        opt.PermitLimit = 30;
        opt.Window = TimeSpan.FromMinutes(1);
        opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        opt.QueueLimit = 0;
    });

    options.AddFixedWindowLimiter("LoginPolicy", opt =>
    {
        opt.PermitLimit = 5;
        opt.Window = TimeSpan.FromMinutes(1);
        opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        opt.QueueLimit = 0;
    });

    options.AddFixedWindowLimiter("BookingPolicy", opt =>
    {
        opt.PermitLimit = 10;
        opt.Window = TimeSpan.FromMinutes(1);
        opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        opt.QueueLimit = 0;
    });

    options.OnRejected = async (context, cancellationToken) =>
    {
        context.HttpContext.Response.StatusCode = 429;
        await context.HttpContext.Response.WriteAsync(
            "Too many requests. Please try again later.", cancellationToken);

        Log.Warning("🚫 Rate limit exceeded for {Path} from IP {IP}",
            context.HttpContext.Request.Path,
            context.HttpContext.Connection.RemoteIpAddress);
    };
});

// Add services to the container.
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options => {
    options.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme {
        Description = "Standard Authorization header using the Bearer scheme (\"bearer {token}\")",
        In = ParameterLocation.Header,
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey
    });
    options.OperationFilter<SecurityRequirementsOperationFilter>();
});
builder.Services.AddApplicationServices(builder.Configuration);
builder.Services.AddIdentityServices(builder.Configuration);
builder.Services.AddCors(options => {
    options.AddPolicy("DevCors", b => b.WithOrigins("http://localhost:4200").AllowAnyMethod().AllowAnyHeader().AllowCredentials());
    options.AddPolicy("ProdCors", b => b.WithOrigins("https://myProductionSite.com").AllowAnyMethod().AllowAnyHeader().AllowCredentials());
});
builder.Services.AddAutoMapper(cfg => 
{
    cfg.AddProfile<MappingProfiles>();
});
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment()) {
    app.UseCors("DevCors");
    app.UseSwagger();
    app.UseSwaggerUI();
} else {
    app.UseCors("ProdCors");
    app.UseHttpsRedirection();
}
app.UseRateLimiter();

app.UseSerilogRequestLogging();
app.UseAuthentication();
app.UseAuthorization();
app.UseMiddleware<EmailVerificationMiddleware>();
app.MapControllers();

try
{
    Log.Information("🚀 Eventify API Starting...");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "❌ Application failed to start");
}
finally
{
    Log.CloseAndFlush();
}