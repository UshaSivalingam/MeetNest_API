using MeetNest.API.Constants;
using MeetNest.API.Handlers;

public static class RoomEndpoints
{
    public static void MapRoomEndpoints(this WebApplication app)
    {
        // Admin routes
        var admin = app.MapGroup("/api/rooms")
                       .WithTags("Rooms")
                       .RequireAuthorization("AdminOnly");

        admin.MapGet("/", RoomHandlers.GetAll).WithName("GetAllRooms");
        admin.MapGet("/{id:int}", RoomHandlers.GetById).WithName("GetRoomById");
        admin.MapGet("/branch/{branchId:int}", RoomHandlers.GetByBranch).WithName("GetRoomsByBranch");
        admin.MapPost("/", RoomHandlers.Create).WithName("CreateRoom");
        admin.MapPut("/{id:int}", RoomHandlers.Update).WithName("UpdateRoom");
        admin.MapDelete("/{id:int}", RoomHandlers.Delete).WithName("DeleteRoom");

        // Employee route (simple list for booking picker)
        app.MapGet("/api/rooms/employee", RoomHandlers.GetEmployeeRooms)
           .WithTags("Rooms")
           .WithName("GetEmployeeRooms")
           .RequireAuthorization("EmployeeOnly");
    }
}
