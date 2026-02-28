using MeetNest.API.Handlers;

namespace MeetNest.API.Endpoints;

public static class BranchEndpoints
{
    public static void MapBranchEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/branches")
                       .WithTags("Branches")
                       .RequireAuthorization("AdminOnly");

        group.MapGet("/", BranchHandlers.GetAll).WithName("GetAllBranches");
        group.MapGet("/simple", BranchHandlers.GetAllSimple).WithName("GetAllBranchesSimple");
        group.MapGet("/{id:int}", BranchHandlers.GetById).WithName("GetBranchById");
        group.MapPost("/", BranchHandlers.Create).WithName("CreateBranch");
        group.MapPut("/{id:int}", BranchHandlers.Update).WithName("UpdateBranch");
        group.MapDelete("/{id:int}", BranchHandlers.Delete).WithName("DeleteBranch");
    }
}
