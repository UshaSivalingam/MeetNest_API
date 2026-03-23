using MeetNest.Application.Interfaces.Services;
using System.Security.Claims;

namespace MeetNest.API.Handlers;

public static class NotificationHandlers
{
    // GET /api/notifications — returns due + unread notifications for the logged-in admin
    public static async Task<IResult> GetDue(
        ClaimsPrincipal user,
        INotificationService service)
    {
        try
        {
            var userId = int.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var result = await service.GetDueForUserAsync(userId);
            return Results.Ok(result);
        }
        catch (Exception ex) { return Results.BadRequest(ex.Message); }
    }

    // GET /api/notifications/count — unread badge count
    public static async Task<IResult> GetCount(
        ClaimsPrincipal user,
        INotificationService service)
    {
        try
        {
            var userId = int.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var count = await service.GetUnreadCountAsync(userId);
            return Results.Ok(new { count });
        }
        catch (Exception ex) { return Results.BadRequest(ex.Message); }
    }

    // PUT /api/notifications/{id}/read
    public static async Task<IResult> MarkRead(
        int id,
        ClaimsPrincipal user,
        INotificationService service)
    {
        try
        {
            var userId = int.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);
            await service.MarkReadAsync(id, userId);
            return Results.Ok(new { message = "Marked as read." });
        }
        catch (Exception ex) { return Results.BadRequest(ex.Message); }
    }

    // PUT /api/notifications/read-all
    public static async Task<IResult> MarkAllRead(
        ClaimsPrincipal user,
        INotificationService service)
    {
        try
        {
            var userId = int.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);
            await service.MarkAllReadAsync(userId);
            return Results.Ok(new { message = "All marked as read." });
        }
        catch (Exception ex) { return Results.BadRequest(ex.Message); }
    }

    // POST /api/notifications/reminder  body: { bookingId }
    public static async Task<IResult> CreateReminder(
        ClaimsPrincipal user,
        INotificationService service,
        CreateReminderRequest body)
    {
        try
        {
            var adminId = int.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);
            await service.CreateReminderAsync(body.BookingId, adminId);
            return Results.Ok(new { message = "Reminder set." });
        }
        catch (Exception ex) { return Results.BadRequest(ex.Message); }
    }

    public record CreateReminderRequest(int BookingId);
}