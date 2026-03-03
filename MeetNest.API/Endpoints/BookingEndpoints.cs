using MeetNest.API.Handlers;
using MeetNest.API.Constants;

namespace MeetNest.API.Endpoints;

public static class BookingEndpoints
{
    public static void MapBookingEndpoints(this WebApplication app)
    {
        // Employee creates a booking
        app.MapPost(ApiRoutes.Booking.Create, BookingHandlers.Create)
           .WithTags("Bookings")
           .RequireAuthorization("EmployeeOnly");

        // Employee views their bookings (paged + filtered)
        app.MapGet(ApiRoutes.Booking.MyBookings, BookingHandlers.MyBookings)
           .WithTags("Bookings")
           .RequireAuthorization("EmployeeOnly");

        // Employee cancels
        app.MapPut(ApiRoutes.Booking.Cancel, BookingHandlers.Cancel)
           .WithTags("Bookings")
           .RequireAuthorization("EmployeeOnly");

        // Admin approve/reject (kept on original routes for backward compat)
        app.MapPut(ApiRoutes.Booking.Approve, BookingHandlers.Approve)
           .WithTags("Bookings")
           .RequireAuthorization("AdminOnly");

        app.MapPut(ApiRoutes.Booking.Reject, BookingHandlers.Reject)
           .WithTags("Bookings")
           .RequireAuthorization("AdminOnly");
    }
}