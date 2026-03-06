using MeetNest.Application.DTOs;
using MeetNest.Application.DTOs.Branch;
using MeetNest.Application.DTOs.Filters;
using MeetNest.Domain.Entities;

namespace MeetNest.Application.Interfaces.Services;

public interface IBranchService
{
    Task<PagedResult<BranchWithStatsDto>> GetAllAsync(BranchFilterDto filter);
    Task<List<Branch>> GetAllSimpleAsync();
    Task<Branch> GetByIdAsync(int id);
    Task CreateAsync(Branch branch);
    Task UpdateAsync(int id, Branch updatedBranch);
    Task DeleteAsync(int id);
}
