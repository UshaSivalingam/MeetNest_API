using MeetNest.Application.DTOs.Filters;
using MeetNest.Application.DTOs.Room;
using MeetNest.Application.Interfaces.Services;
public static class RoomHandlers
{
    // GET /api/rooms
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
    public static async Task<IResult> Delete(int id, IRoomService service)
    {
        try
        {
            await service.DeleteAsync(id);
            return Results.Ok(new { message = "Room deleted." });
        }
        catch (Exception ex) when (ex.Message.StartsWith("ROOM_HAS_ACTIVE_BOOKINGS"))
        {
            return Results.Conflict(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            return Results.BadRequest(new { error = ex.Message });
        }
    }

    // GET /api/rooms/{id}/active-bookings
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

    // PUT /api/rooms/{id}/block
    // Body: { "blockFromDate": "2026-04-08", "reason": "Maintenance" | "Deletion" }
    public static async Task<IResult> SetBlock(int id, SetBlockDto dto, IRoomService service)
    {
        try
        {
            await service.SetBlockDateAsync(id, dto.BlockFromDate, dto.Reason);
            return Results.Ok(new { message = $"Room blocked from {dto.BlockFromDate:dd MMM yyyy} — {dto.Reason}." });
        }
        catch (Exception ex)
        {
            return Results.BadRequest(new { error = ex.Message });
        }
    }

    // DELETE /api/rooms/{id}/block
    public static async Task<IResult> RemoveBlock(int id, IRoomService service)
    {
        try
        {
            await service.RemoveBlockDateAsync(id);
            return Results.Ok(new { message = "Block date removed. Room is fully open for bookings." });
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

// ── DTO for block request body ────────────────────────────────────
public record SetBlockDto(DateTime BlockFromDate, string Reason);