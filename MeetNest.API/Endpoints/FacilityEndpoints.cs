using MeetNest.API.Constants;
using MeetNest.API.Handlers;

namespace MeetNest.API.Endpoints;

public static class FacilityEndpoints
{
    public static void MapFacilityEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/facilities")
                       .WithTags("Facilities")
                       .RequireAuthorization("AdminOnly");

        group.MapGet("/", FacilityHandlers.GetAll).WithName("GetAllFacilities");
        group.MapGet("/simple", FacilityHandlers.GetAllSimple).WithName("GetAllFacilitiesSimple");
        group.MapGet("/{id:int}", FacilityHandlers.GetById).WithName("GetFacilityById");
        group.MapPost("/", FacilityHandlers.Create).WithName("CreateFacility");
        group.MapPut("/{id:int}", FacilityHandlers.Update).WithName("UpdateFacility");
        group.MapDelete("/{id:int}", FacilityHandlers.Delete).WithName("DeleteFacility");
    }
}
