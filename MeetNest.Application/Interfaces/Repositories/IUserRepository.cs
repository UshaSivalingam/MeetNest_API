using MeetNest.Application.DTOs;
using MeetNest.Application.DTOs.Filters;
using MeetNest.Domain.Entities;

namespace MeetNest.Application.Interfaces.Repositories;

public interface IUserRepository
{
    Task AddAsync(User user);
    Task<User?> GetByIdAsync(int id);
    Task<User?> GetByEmailAsync(string email);
    Task UpdateAsync(User user);
    Task DeleteAsync(User user);
    Task<bool> AdminExistsAsync();
    Task<int> SaveChangesAsync();
    Task<PagedResult<User>> GetAllAsync(UserFilterDto filter);
    Task<PagedResult<User>> GetAllEmployeesAsync(UserFilterDto filter);
    Task<Branch?> GetBranchByIdAsync(int branchId);

}
