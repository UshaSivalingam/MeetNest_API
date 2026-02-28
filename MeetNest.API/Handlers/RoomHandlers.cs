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

    // GET /api/rooms/branch/{branchId}?search=&page=1&pageSize=10
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

    public static async Task<IResult> Delete(int id, IRoomService service)
    {
        await service.DeleteAsync(id);
        return Results.Ok();
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
