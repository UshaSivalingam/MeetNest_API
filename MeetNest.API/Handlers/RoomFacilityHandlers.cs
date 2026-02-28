using MeetNest.Application.DTOs.RoomFacility;
using MeetNest.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace MeetNest.API.Handlers;

public static class RoomFacilityHandlers
{
    public static async Task<IResult> Assign(
        CreateRoomFacilityDto dto,
        IRoomFacilityService service)
    {
        await service.AssignFacilityAsync(dto);
        return Results.Ok("Assigned successfully.");
    }

    public static async Task<IResult> GetByRoom(
        int roomId,
        IRoomFacilityService service)
    {
        var result = await service.GetFacilitiesByRoomAsync(roomId);
        return Results.Ok(result);
    }
    public static async Task<IResult> Remove(
        [FromBody] RemoveRoomFacilityDto dto,
        [FromServices] IRoomFacilityService service)
    {
        var removed = await service
            .RemoveFacilityAsync(dto.RoomId, dto.FacilityId);

        if (!removed)
            return Results.NotFound("Assignment not found.");

        return Results.Ok("Removed successfully.");
    }
}
