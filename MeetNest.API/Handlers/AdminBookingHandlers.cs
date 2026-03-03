using System.Security.Claims;
using MeetNest.Application.DTOs.Admin;
using MeetNest.Application.Interfaces.Services;

namespace MeetNest.API.Handlers;

public static class AdminBookingHandlers
{
    // GET /api/admin/bookings?status=Pending&branchId=1&from=&to=&page=1&pageSize=10
    public static async Task<IResult> GetAll(
        IAdminBookingService service,
        string? status = null,
        int? branchId = null,
        DateTime? from = null,
        DateTime? to = null,
        int page = 1,
        int pageSize = 10)
    {
        try
        {
            var filter = new AdminBookingFilterDto
            {
                Status = status,
                BranchId = branchId,
                From = from,
                To = to,
                Page = page,
                PageSize = pageSize
            };

            var result = await service.GetAllAsync(filter);
            return Results.Ok(result);
        }
        catch (Exception ex)
        {
            return Results.BadRequest(ex.Message);
        }
    }

    // GET /api/admin/bookings/{id}
    public static async Task<IResult> GetById(int id, IAdminBookingService service)
    {
        try
        {
            var booking = await service.GetByIdAsync(id);
            return booking is null
                ? Results.NotFound("Booking not found.")
                : Results.Ok(booking);
        }
        catch (Exception ex)
        {
            return Results.BadRequest(ex.Message);
        }
    }

    // PUT /api/admin/bookings/{id}/approve
    public static async Task<IResult> Approve(
        int id,
        BookingActionBodyDto body,
        ClaimsPrincipal user,
        IAdminBookingService service)
    {
        try
        {
            var adminId = int.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);
            await service.ApproveAsync(id, adminId, body);
            return Results.Ok(new { Message = "Booking approved successfully." });
        }
        catch (Exception ex)
        {
            return Results.BadRequest(ex.Message);
        }
    }

    // PUT /api/admin/bookings/{id}/reject
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
}
