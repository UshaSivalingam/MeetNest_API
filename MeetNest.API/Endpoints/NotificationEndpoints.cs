using MeetNest.API.Handlers;

namespace MeetNest.API.Endpoints;

public static class NotificationEndpoints
{
    public static void MapNotificationEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/notifications")
                       .WithTags("Notifications")
                       .RequireAuthorization("AdminOnly");

        // GET /api/notifications — due + unread notifications
        group.MapGet("/", NotificationHandlers.GetDue)
             .WithName("GetNotifications");

        // GET /api/notifications/count — bell badge number
        // ⚠️ MUST be registered before /{id:int} to avoid route conflict
        group.MapGet("/count", NotificationHandlers.GetCount)
             .WithName("GetNotificationCount");

        // PUT /api/notifications/read-all
        // ⚠️ MUST be registered before /{id:int}/read
        group.MapPut("/read-all", NotificationHandlers.MarkAllRead)
             .WithName("MarkAllNotificationsRead");

        // PUT /api/notifications/{id}/read
        group.MapPut("/{id:int}/read", NotificationHandlers.MarkRead)
             .WithName("MarkNotificationRead");

        // POST /api/notifications/reminder — admin manually sets end-time reminder
        group.MapPost("/reminder", NotificationHandlers.CreateReminder)
             .WithName("CreateNotificationReminder");
    }
}