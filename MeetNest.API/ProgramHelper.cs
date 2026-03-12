using MeetNest.API.Endpoints;
using MeetNest.API.Middleware;
using MeetNest.Application.Interfaces.Repositories;
using MeetNest.Application.Interfaces.Security;
using MeetNest.Application.Interfaces.Services;
using MeetNest.Infrastructure.Data;
using MeetNest.Infrastructure.Repositories;
using MeetNest.Infrastructure.Security;
using MeetNest.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Text.Json.Serialization;

namespace MeetNest.API;

public static class ProgramHelper
{
    public static void AddApplicationServices(
        this IServiceCollection services,
        IConfiguration config)
    {
        // -------------------- CONTROLLERS --------------------
        services.AddControllers()
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions
                    .Converters.Add(new JsonStringEnumConverter());
            });

        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();

        // -------------------- DATABASE --------------------
        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(
                config.GetConnectionString("DefaultConnection")));

        // -------------------- REPOSITORIES --------------------
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IBranchRepository, BranchRepository>();
        services.AddScoped<IRoomRepository, RoomRepository>();
        services.AddScoped<IFacilityRepository, FacilityRepository>();
        services.AddScoped<IRoomFacilityRepository, RoomFacilityRepository>();
        services.AddScoped<IBookingRepository, BookingRepository>();
        services.AddScoped<INotificationRepository, NotificationRepository>();

        // -------------------- SERVICES --------------------
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IBranchService, BranchService>();
        services.AddScoped<IRoomService, RoomService>();
        services.AddScoped<IFacilityService, FacilityService>();
        services.AddScoped<IRoomFacilityService, RoomFacilityService>();
        services.AddScoped<IBookingService, BookingService>();

        // -------------------- ADMIN SERVICES --------------------
        services.AddScoped<IAdminDashboardService, AdminDashboardService>();
        services.AddScoped<IAdminBookingService, AdminBookingService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<INotificationService, NotificationService>();

        // -------------------- SECURITY --------------------
        services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
        services.AddScoped<PasswordHasher>();

        // -------------------- JWT --------------------
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters =
                    new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,

                        ValidIssuer = config["Jwt:Issuer"],
                        ValidAudience = config["Jwt:Audience"],

                        IssuerSigningKey =
                            new SymmetricSecurityKey(
                                Encoding.UTF8.GetBytes(config["Jwt:Key"]!))
                    };
            });

        // -------------------- CORS --------------------
        services.AddCors(options =>
        {
            options.AddPolicy("AllowReact", policy =>
            {
                policy.WithOrigins("http://localhost:5173")
                      .AllowAnyHeader()
                      .AllowAnyMethod();
            });
        });

        // -------------------- AUTHORIZATION --------------------
        services.AddAuthorization(options =>
        {
            options.AddPolicy("AdminOnly",
                policy => policy.RequireRole("Admin"));

            options.AddPolicy("EmployeeOnly",
                policy => policy.RequireRole("Employee"));
        });
    }

    // ---------------------------------------------------------
    // PIPELINE
    // ---------------------------------------------------------
    public static void UseApplicationPipeline(this WebApplication app)
    {
        app.UseSwagger();
        app.UseSwaggerUI();

        app.UseMiddleware<GlobalExceptionMiddleware>();

        app.UseCors("AllowReact");

        app.UseAuthentication();
        app.UseAuthorization();

        // -------------------- EMPLOYEE ENDPOINTS --------------------
        app.MapAuthEndpoints();
        app.MapBranchEndpoints();
        app.MapRoomEndpoints();
        app.MapNotificationEndpoints();
        app.MapFacilityEndpoints();
        app.MapRoomFacilityEndpoints();
        app.MapBookingEndpoints();

        // -------------------- ADMIN ENDPOINTS --------------------
        app.MapAdminDashboardEndpoints();
        app.MapAdminBookingEndpoints();
        app.MapUserEndpoints();
    }
}