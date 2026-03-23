using MeetNest.API.Handlers;

public static class RoomEndpoints
{
    public static void MapRoomEndpoints(this WebApplication app)
    {
        var admin = app.MapGroup("/api/rooms")
                       .WithTags("Rooms")
                       .RequireAuthorization("AdminOnly");

        // ⚠️ ORDER MATTERS — specific literal routes before {id:int}
        admin.MapGet("/", RoomHandlers.GetAll).WithName("GetAllRooms");
        admin.MapGet("/branch/{branchId:int}", RoomHandlers.GetByBranch).WithName("GetRoomsByBranch");
        admin.MapGet("/{id:int}", RoomHandlers.GetById).WithName("GetRoomById");
        admin.MapPost("/", RoomHandlers.Create).WithName("CreateRoom");
        admin.MapPut("/{id:int}", RoomHandlers.Update).WithName("UpdateRoom");
        admin.MapDelete("/{id:int}", RoomHandlers.Delete).WithName("DeleteRoom");

        // Check active bookings before delete/maintenance
        admin.MapGet("/{id:int}/active-bookings", RoomHandlers.GetActiveBookings)
             .WithName("GetRoomActiveBookings");

        // ── Block scheduling ──────────────────────────────────────
        // PUT  /api/rooms/{id}/block  — set block date + reason
        // DELETE /api/rooms/{id}/block — remove block
        admin.MapPut("/{id:int}/block", RoomHandlers.SetBlock).WithName("SetRoomBlock");
        admin.MapDelete("/{id:int}/block", RoomHandlers.RemoveBlock).WithName("RemoveRoomBlock");

        // Employee route — any authenticated user in their branch
        app.MapGet("/api/rooms/employee", RoomHandlers.GetEmployeeRooms)
           .WithTags("Rooms")
           .WithName("GetEmployeeRooms")
           .RequireAuthorization("EmployeeOnly");
    }
}