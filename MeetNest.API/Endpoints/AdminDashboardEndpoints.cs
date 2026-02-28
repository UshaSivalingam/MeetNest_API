using MeetNest.API.Constants;
using MeetNest.API.Handlers;

namespace MeetNest.API.Endpoints;

public static class AdminDashboardEndpoints
{
    public static void MapAdminDashboardEndpoints(this WebApplication app)
    {
        app.MapGet(ApiRoutes.Admin.Dashboard, AdminDashboardHandlers.GetStats)
           .WithName("GetDashboardStats")
           .WithTags("Admin - Dashboard")
           .RequireAuthorization("AdminOnly");
    }
}
