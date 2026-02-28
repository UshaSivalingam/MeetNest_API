using MeetNest.Application.DTOs.Admin;

namespace MeetNest.Application.Interfaces.Services;

public interface IAdminDashboardService
{
    Task<DashboardStatsDto> GetStatsAsync();
}
