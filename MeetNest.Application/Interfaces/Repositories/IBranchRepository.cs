
using MeetNest.Application.DTOs;
using MeetNest.Application.DTOs.Branch;
using MeetNest.Application.DTOs.Filters;
using MeetNest.Domain.Entities;

namespace MeetNest.Application.Interfaces.Repositories;

public interface IBranchRepository
{
    // ── Existing (unchanged) ──────────────────────────────────────────────────
    Task<Branch?> GetByIdAsync(int id);
    Task AddAsync(Branch branch);
    void Update(Branch branch);
    Task SaveChangesAsync();
    Task<bool> ExistsAsync(int branchId);
    Task<bool> ExistsAsync(string name, string city);
    Task<List<BranchWithStatsDto>> GetAllWithStatsAsync();

    // ── Updated: was GetAllAsync() → now paged + filtered ────────────────────
    Task<PagedResult<Branch>> GetAllAsync(BranchFilterDto filter);

    // ── Keep simple list for dropdowns (no pagination needed) ─────────────────
    Task<List<Branch>> GetAllSimpleAsync();
}

