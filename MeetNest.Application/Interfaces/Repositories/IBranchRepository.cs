using MeetNest.Application.DTOs;
using MeetNest.Application.DTOs.Branch;
using MeetNest.Application.DTOs.Filters;
using MeetNest.Domain.Entities;

namespace MeetNest.Application.Interfaces.Repositories;

public interface IBranchRepository
{
    Task<Branch?> GetByIdAsync(int id);
    Task AddAsync(Branch branch);
    void Update(Branch branch);
    Task SaveChangesAsync();
    Task<bool> ExistsAsync(int branchId);
    Task<bool> ExistsAsync(string name, string city);

    // Paged + filtered — returns DTO with stats
    Task<PagedResult<BranchWithStatsDto>> GetAllAsync(BranchFilterDto filter);

    // Simple flat list for dropdowns
    Task<List<Branch>> GetAllSimpleAsync();

    // Stats list (no pagination) — kept for dashboard/reports
    Task<List<BranchWithStatsDto>> GetAllWithStatsAsync();
}
