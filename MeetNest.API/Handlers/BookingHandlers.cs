using MeetNest.Application.DTOs.Admin;
using MeetNest.Application.DTOs.Booking;
using MeetNest.Application.DTOs.Filters;
using MeetNest.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace MeetNest.API.Handlers;

public static class BookingHandlers
{
    public static async Task<IResult> Create(
        ClaimsPrincipal user,
         [FromBody] CreateBookingDto dto,
        IBookingService bookingService)
    {
        try
        {
            var employeeId = int.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var bookingId = await bookingService.CreateAsync(employeeId, dto);
            return Results.Ok(new { Message = "Booking created.", BookingId = bookingId });
        }
        catch (Exception ex) { return Results.BadRequest(ex.Message); }
    }

    // GET /api/bookings/my?status=&search=&from=&to=&page=1&pageSize=10
    public static async Task<IResult> MyBookings(
        ClaimsPrincipal user,
        IBookingService bookingService,
        string? status = null,
        string? search = null,
        DateTime? from = null,
        DateTime? to = null,
        int page = 1,
        int pageSize = 10)
    {
        try
        {
            var employeeId = int.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var filter = new MyBookingFilterDto
            {
                Status = status,
                Search = search,
                From = from,
                To = to,
                Page = page,
                PageSize = pageSize
            };
            var bookings = await bookingService.GetMyBookingsAsync(employeeId, filter);
            return Results.Ok(bookings);
        }
        catch (Exception ex) { return Results.BadRequest(ex.Message); }
    }

    public static async Task<IResult> Approve(
        int id, ClaimsPrincipal user, IBookingService bookingService,
        string? overrideReason = null, bool force = false)
    {
        try
        {
            var adminId = int.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);
            await bookingService.ApproveAsync(id, adminId, force, overrideReason);
            return Results.Ok(new { Message = "Booking approved." });
        }
        catch (Exception ex) { return Results.BadRequest(ex.Message); }
    }

    public static async Task<IResult> Reject(
        int id,
        BookingActionBodyDto body,
        ClaimsPrincipal user,
        IAdminBookingService service)
    {
        try
        {
            var adminId = int.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);
            await service.RejectAsync(id, adminId, body);   // ✅ full body, not body.Reason
            return Results.Ok(new { Message = "Booking rejected." });
        }
        catch (Exception ex)
        {
            return Results.BadRequest(ex.Message);
        }
    }

    public static async Task<IResult> Cancel(
        int id, ClaimsPrincipal user, IBookingService bookingService)
    {
        try
        {
            var employeeId = int.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);
            await bookingService.CancelAsync(id, employeeId);
            return Results.Ok(new { Message = "Booking cancelled." });
        }
        catch (Exception ex) { return Results.BadRequest(ex.Message); }
    }
}