using MeetNest.API.Constants;
using MeetNest.API.Handlers;

namespace MeetNest.API.Endpoints;

public static class AdminBookingEndpoints
{
    public static void MapAdminBookingEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/admin/bookings")
                       .WithTags("Admin - Bookings")
                       .RequireAuthorization("AdminOnly");

        // GET /api/admin/bookings?status=Pending&branchId=1&from=...&to=...&page=1&pageSize=10
        group.MapGet("/", AdminBookingHandlers.GetAll)
             .WithName("AdminGetAllBookings");

        // GET /api/admin/bookings/{id}
        group.MapGet("/{id:int}", AdminBookingHandlers.GetById)
             .WithName("AdminGetBookingById");

        // PUT /api/admin/bookings/{id}/approve   body: { reason, force }
        group.MapPut("/{id:int}/approve", AdminBookingHandlers.Approve)
             .WithName("AdminApproveBooking");

        // PUT /api/admin/bookings/{id}/reject    body: { reason }
        group.MapPut("/{id:int}/reject", AdminBookingHandlers.Reject)
             .WithName("AdminRejectBooking");
    }
}
