
using MeetNest.API.Handlers;

public static class RoomEndpoints
{
    public static void MapRoomEndpoints(this WebApplication app)
    {
        var admin = app.MapGroup("/api/rooms")
                       .WithTags("Rooms")
                       .RequireAuthorization("AdminOnly");

        // ⚠️  ORDER MATTERS — specific literal routes must come before {id:int}
        admin.MapGet("/", RoomHandlers.GetAll).WithName("GetAllRooms");
        admin.MapGet("/branch/{branchId:int}", RoomHandlers.GetByBranch).WithName("GetRoomsByBranch");
        admin.MapGet("/{id:int}", RoomHandlers.GetById).WithName("GetRoomById");
        admin.MapPost("/", RoomHandlers.Create).WithName("CreateRoom");
        admin.MapPut("/{id:int}", RoomHandlers.Update).WithName("UpdateRoom");
        admin.MapDelete("/{id:int}", RoomHandlers.Delete).WithName("DeleteRoom");

        // ── NEW: check active bookings before confirming delete ───
        // ⚠️ Must be registered before /{id:int} route group closes
        admin.MapGet("/{id:int}/active-bookings", RoomHandlers.GetActiveBookings)
             .WithName("GetRoomActiveBookings");

        // Employee route — separate from admin group, uses EmployeeOnly auth
        app.MapGet("/api/rooms/employee", RoomHandlers.GetEmployeeRooms)
           .WithTags("Rooms")
           .WithName("GetEmployeeRooms")
           .RequireAuthorization("EmployeeOnly");
    }
}