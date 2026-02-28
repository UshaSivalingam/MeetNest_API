using MeetNest.API.Handlers;

namespace MeetNest.API.Endpoints;

public static class UserEndpoints
{
    public static void MapUserEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/admin/users")
                       .WithTags("Admin - Users")
                       .RequireAuthorization("AdminOnly");

        // GET /api/admin/users
        group.MapGet("/", UserHandlers.GetAll)
             .WithName("AdminGetAllUsers");

        // GET /api/admin/users/employees
        group.MapGet("/employees", UserHandlers.GetAllEmployees)
             .WithName("AdminGetAllEmployees");

        // GET /api/admin/users/{id}
        group.MapGet("/{id:int}", UserHandlers.GetById)
             .WithName("AdminGetUserById");

        // PUT /api/admin/users/{id}
        group.MapPut("/{id:int}", UserHandlers.Update)
             .WithName("AdminUpdateUser");

        // PUT /api/admin/users/{id}/deactivate
        group.MapPut("/{id:int}/deactivate", UserHandlers.Deactivate)
             .WithName("AdminDeactivateUser");

        // PUT /api/admin/users/{id}/reset-password
        group.MapPut("/{id:int}/reset-password", UserHandlers.ResetPassword)
             .WithName("AdminResetUserPassword");
    }
}
