using MeetNest.API.Constants;
using MeetNest.API.Handlers;
using MeetNest.Application.DTOs.Auth;

namespace MeetNest.API.Endpoints;

public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this WebApplication app)
    {
        app.MapPost(ApiRoutes.Auth.RegisterAdmin,
            AuthHandlers.RegisterAdmin)
            .WithName("RegisterAdmin")
            .WithTags("Authentication");

        app.MapPost(ApiRoutes.Auth.RegisterEmployee,
            AuthHandlers.RegisterEmployee)
            .WithName("RegisterEmployee")
            .WithTags("Authentication");

        app.MapPost(ApiRoutes.Auth.Login,
            AuthHandlers.Login)
            .WithName("Login")
            .WithTags("Authentication");

        // Future implementation
        app.MapPost(ApiRoutes.Auth.RefreshToken,
            (RefreshTokenDto dto) =>
            {
                return Results.Ok("Refresh token endpoint coming soon.");
            })
            .WithName("RefreshToken")
            .WithTags("Authentication");
    }
}