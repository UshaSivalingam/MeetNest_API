using MeetNest.Application.DTOs;
using MeetNest.Application.DTOs.Filters;
using MeetNest.Domain.Entities;

namespace MeetNest.Application.Interfaces.Services;

public interface IBranchService
{
    Task<PagedResult<Branch>> GetAllAsync(BranchFilterDto filter);
    Task<List<Branch>> GetAllSimpleAsync();          // for dropdowns
    Task<Branch> GetByIdAsync(int id);
    Task CreateAsync(Branch branch);
    Task UpdateAsync(int id, Branch updatedBranch);
    Task DeleteAsync(int id);
}
