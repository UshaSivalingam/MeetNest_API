using MeetNest.API.Handlers;
using MeetNest.API.Constants;

namespace MeetNest.API.Endpoints;

public static class BookingEndpoints
{
    public static void MapBookingEndpoints(this WebApplication app)
    {
        // Employee creates a booking
        app.MapPost("/api/bookings", BookingHandlers.Create)
           .WithTags("Bookings").RequireAuthorization("EmployeeOnly");

        // Employee views their bookings (paged + filtered)
        app.MapGet("/api/bookings/my", BookingHandlers.MyBookings)
           .WithTags("Bookings").RequireAuthorization("EmployeeOnly");

        // Employee cancels
        app.MapPut("/api/bookings/{id}/cancel", BookingHandlers.Cancel)
           .WithTags("Bookings").RequireAuthorization("EmployeeOnly");

        // Admin approve/reject (kept on original routes for backward compat)
        app.MapPut("/api/bookings/{id}/approve", BookingHandlers.Approve)
           .WithTags("Bookings").RequireAuthorization("AdminOnly");

        app.MapPut("/api/bookings/{id}/reject", BookingHandlers.Reject)
           .WithTags("Bookings").RequireAuthorization("AdminOnly");
    }
}
