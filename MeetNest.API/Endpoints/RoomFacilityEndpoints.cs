using MeetNest.API.Constants;
using MeetNest.API.Handlers;

namespace MeetNest.API.Endpoints;

public static class RoomFacilityEndpoints
{
    public static void MapRoomFacilityEndpoints(this WebApplication app)
    {
        app.MapPost(ApiRoutes.RoomFacility.Assign,
            RoomFacilityHandlers.Assign).RequireAuthorization("AdminOnly"); ;

        app.MapGet(ApiRoutes.RoomFacility.GetByRoom,
            RoomFacilityHandlers.GetByRoom);

        app.MapDelete(ApiRoutes.RoomFacility.Remove,
            RoomFacilityHandlers.Remove)
            .RequireAuthorization("AdminOnly");

    }
}
