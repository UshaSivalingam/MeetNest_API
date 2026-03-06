using MeetNest.Application.DTOs;
using MeetNest.Application.DTOs.Branch;
using MeetNest.Application.DTOs.Filters;
using MeetNest.Application.Interfaces.Repositories;
using MeetNest.Application.Interfaces.Services;
using MeetNest.Domain.Entities;

namespace MeetNest.Infrastructure.Services;

public class BranchService : IBranchService
{
    private readonly IBranchRepository _repo;
    public BranchService(IBranchRepository repo) => _repo = repo;

    public Task<PagedResult<BranchWithStatsDto>> GetAllAsync(BranchFilterDto filter)
        => _repo.GetAllAsync(filter);

    public Task<List<Branch>> GetAllSimpleAsync()
        => _repo.GetAllSimpleAsync();

    public async Task<Branch> GetByIdAsync(int id)
    {
        var branch = await _repo.GetByIdAsync(id);
        if (branch == null || !branch.IsActive) throw new Exception("Branch not found.");
        return branch;
    }

    public async Task CreateAsync(Branch branch)
    {
        if (string.IsNullOrWhiteSpace(branch.Name)) throw new Exception("Branch name is required.");
        if (string.IsNullOrWhiteSpace(branch.City)) throw new Exception("City is required.");
        if (await _repo.ExistsAsync(branch.Name, branch.City))
            throw new Exception("Branch already exists in this city.");

        branch.IsActive = true;
        branch.CreatedAt = DateTime.UtcNow;
        await _repo.AddAsync(branch);
        await _repo.SaveChangesAsync();
    }

    public async Task UpdateAsync(int id, Branch updated)
    {
        var existing = await _repo.GetByIdAsync(id);
        if (existing == null || !existing.IsActive) throw new Exception("Branch not found.");

        var dupExists = await _repo.ExistsAsync(updated.Name, updated.City);
        if (dupExists &&
            (existing.Name.ToLower() != updated.Name.ToLower() ||
             existing.City.ToLower() != updated.City.ToLower()))
            throw new Exception("Another branch with this name already exists in this city.");

        existing.Name = updated.Name;
        existing.Area = updated.Area;
        existing.City = updated.City;
        existing.Country = updated.Country;
        existing.UpdatedAt = DateTime.UtcNow;
        _repo.Update(existing);
        await _repo.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var branch = await _repo.GetByIdAsync(id);
        if (branch == null || !branch.IsActive) throw new Exception("Branch not found.");
        branch.IsActive = false;
        branch.UpdatedAt = DateTime.UtcNow;
        _repo.Update(branch);
        await _repo.SaveChangesAsync();
    }
}
