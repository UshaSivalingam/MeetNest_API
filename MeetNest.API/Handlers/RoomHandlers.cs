// MeetNest.API/Handlers/RoomHandlers.cs
// ── REPLACE your existing file entirely ──

using MeetNest.Application.DTOs.Filters;
using MeetNest.Application.DTOs.Room;
using MeetNest.Application.Interfaces.Services;
using System.Security.Claims;

public static class RoomHandlers
{
    // GET /api/rooms?search=&branchId=&minCap=&maxCap=&page=1&pageSize=10
    public static async Task<IResult> GetAll(
        IRoomService service,
        string? search = null,
        int? branchId = null,
        int? minCap = null,
        int? maxCap = null,
        int page = 1,
        int pageSize = 10)
    {
        var filter = new RoomFilterDto
        {
            Search = search,
            BranchId = branchId,
            MinCap = minCap,
            MaxCap = maxCap,
            Page = page,
            PageSize = pageSize
        };
        return Results.Ok(await service.GetAllAsync(filter));
    }

    // GET /api/rooms/branch/{branchId}
    public static async Task<IResult> GetByBranch(
        int branchId,
        IRoomService service,
        string? search = null,
        int? minCap = null,
        int? maxCap = null,
        int page = 1,
        int pageSize = 10)
    {
        var filter = new RoomFilterDto
        {
            Search = search,
            MinCap = minCap,
            MaxCap = maxCap,
            Page = page,
            PageSize = pageSize
        };
        return Results.Ok(await service.GetByBranchAsync(branchId, filter));
    }

    public static async Task<IResult> GetById(int id, IRoomService service)
    {
        var room = await service.GetByIdAsync(id);
        return room is null ? Results.NotFound() : Results.Ok(room);
    }

    public static async Task<IResult> Create(CreateRoomDto dto, IRoomService service)
    {
        var id = await service.CreateAsync(dto);
        return Results.Created($"/api/rooms/{id}", id);
    }

    public static async Task<IResult> Update(int id, UpdateRoomDto dto, IRoomService service)
    {
        await service.UpdateAsync(id, dto);
        return Results.Ok();
    }

    // DELETE /api/rooms/{id}
    // Returns 409 Conflict if room has active bookings, with structured error body.
    // Frontend reads the error message to detect "ROOM_HAS_ACTIVE_BOOKINGS" prefix.
    public static async Task<IResult> Delete(int id, IRoomService service)
    {
        try
        {
            await service.DeleteAsync(id);
            return Results.Ok(new { message = "Room deleted." });
        }
        catch (Exception ex) when (ex.Message.StartsWith("ROOM_HAS_ACTIVE_BOOKINGS"))
        {
            // 409 Conflict — frontend knows to show the protection modal
            return Results.Conflict(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            return Results.BadRequest(new { error = ex.Message });
        }
    }

    // ── NEW: GET /api/rooms/{id}/active-bookings ──────────────────
    // Called by frontend BEFORE showing delete confirmation.
    // Returns active booking summaries so the warning modal can show detail.
    public static async Task<IResult> GetActiveBookings(int id, IRoomService service)
    {
        try
        {
            var bookings = await service.GetActiveBookingsAsync(id);
            return Results.Ok(bookings);
        }
        catch (Exception ex)
        {
            return Results.BadRequest(new { error = ex.Message });
        }
    }

    // Employee — simple list for booking picker
    public static async Task<IResult> GetEmployeeRooms(HttpContext context, IRoomService service)
    {
        var branchIdClaim = context.User.FindFirst("BranchId")?.Value;
        if (branchIdClaim is null) return Results.Unauthorized();
        var rooms = await service.GetByBranchIdSimpleAsync(int.Parse(branchIdClaim));
        return Results.Ok(rooms);
    }
}