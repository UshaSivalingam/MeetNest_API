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

namespace MeetNest.API;

public static class ProgramHelper
{
    public static void AddApplicationServices(
        this IServiceCollection services,
        IConfiguration config)
    {
        // ── Database ──────────────────────────────────────────────────────────
        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(config.GetConnectionString("DefaultConnection")));

        // ── Repositories ──────────────────────────────────────────────────────
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IBranchRepository, BranchRepository>();
        services.AddScoped<IRoomRepository, RoomRepository>();
        services.AddScoped<IFacilityRepository, FacilityRepository>();
        services.AddScoped<IRoomFacilityRepository, RoomFacilityRepository>();
        services.AddScoped<IBookingRepository, BookingRepository>();

        // ── Services (Employee / Shared) ──────────────────────────────────────
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IBranchService, BranchService>();
        services.AddScoped<IRoomService, RoomService>();
        services.AddScoped<IFacilityService, FacilityService>();
        services.AddScoped<IRoomFacilityService, RoomFacilityService>();
        services.AddScoped<IBookingService, BookingService>();

        // ── Services (Admin — NEW) ────────────────────────────────────────────
        services.AddScoped<IAdminDashboardService, AdminDashboardService>();
        services.AddScoped<IAdminBookingService, AdminBookingService>();
        services.AddScoped<IUserService, UserService>();

        // ── Security ──────────────────────────────────────────────────────────
        services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
        services.AddScoped<PasswordHasher>();

        // ── JWT Authentication ────────────────────────────────────────────────
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = config["Jwt:Issuer"],
                    ValidAudience = config["Jwt:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(
                                                 Encoding.UTF8.GetBytes(config["Jwt:Key"]!))
                };
            });

        // ── CORS ──────────────────────────────────────────────────────────────
        services.AddCors(options =>
        {
            options.AddPolicy("AllowReact", policy =>
            {
                policy.WithOrigins("http://localhost:5173") // React Vite URL
                      .AllowAnyHeader()
                      .AllowAnyMethod();
            });
        });

        // ── Role-based Authorization ──────────────────────────────────────────
        services.AddAuthorization(options =>
        {
            options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
            options.AddPolicy("EmployeeOnly", policy => policy.RequireRole("Employee"));
        });
    }
}
