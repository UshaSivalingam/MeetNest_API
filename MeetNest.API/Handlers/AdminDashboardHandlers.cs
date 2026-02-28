using MeetNest.Application.Interfaces.Services;

namespace MeetNest.API.Handlers;

public static class AdminDashboardHandlers
{
    public static async Task<IResult> GetStats(IAdminDashboardService service)
    {
        try
        {
            var stats = await service.GetStatsAsync();
            return Results.Ok(stats);
        }
        catch (Exception ex)
        {
            return Results.BadRequest(ex.Message);
        }
    }
}
