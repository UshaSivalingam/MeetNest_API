using MeetNest.Application.DTOs;
using MeetNest.Application.DTOs.Filters;
using MeetNest.Application.Interfaces.Repositories;
using MeetNest.Domain.Entities;
using MeetNest.Domain.Enums;
using MeetNest.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace MeetNest.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly AppDbContext _context;
    public UserRepository(AppDbContext context) => _context = context;

    // ── Base query ────────────────────────────────────────────────────────────
    private IQueryable<User> BaseQuery() =>
        _context.Users
            .Include(u => u.Role)
            .Include(u => u.Branch);

    private static IQueryable<User> ApplyFilter(IQueryable<User> query, UserFilterDto filter)
    {
        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var s = filter.Search.Trim().ToLower();
            query = query.Where(u =>
                u.FullName.ToLower().Contains(s) ||
                u.Email.ToLower().Contains(s));
        }

        if (filter.BranchId.HasValue)
            query = query.Where(u => u.BranchId == filter.BranchId.Value);

        if (filter.IsActive.HasValue)
            query = query.Where(u => u.IsActive == filter.IsActive.Value);

        if (!string.IsNullOrWhiteSpace(filter.Role))
        {
            var role = filter.Role.Trim().ToLower();
            query = query.Where(u => u.Role.Name.ToString().ToLower() == role);
        }

        return query;
    }

    private static async Task<PagedResult<User>> ToPagedAsync(
        IQueryable<User> query, int page, int pageSize)
    {
        var total = await query.CountAsync();
        var items = await query
            .OrderBy(u => u.FullName)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResult<User>
        {
            Items = items,
            TotalCount = total,
            Page = page,
            PageSize = pageSize
        };
    }

    // ── Paged: all users ──────────────────────────────────────────────────────
    public async Task<PagedResult<User>> GetAllAsync(UserFilterDto filter)
    {
        var query = ApplyFilter(BaseQuery().Where(u => u.IsActive), filter);
        return await ToPagedAsync(query, filter.Page, filter.PageSize);
    }

    // ── Paged: employees only ─────────────────────────────────────────────────
    public async Task<PagedResult<User>> GetAllEmployeesAsync(UserFilterDto filter)
    {
        var query = ApplyFilter(
            BaseQuery().Where(u => u.IsActive && u.Role.Name == UserRole.Employee),
            filter);
        return await ToPagedAsync(query, filter.Page, filter.PageSize);
    }

    // ── Existing helpers ──────────────────────────────────────────────────────
    public async Task AddAsync(User user)
    {
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();
    }

    public async Task<User?> GetByIdAsync(int id)
        => await BaseQuery().FirstOrDefaultAsync(u => u.Id == id);

    public async Task<User?> GetByEmailAsync(string email)
        => await BaseQuery().FirstOrDefaultAsync(u => u.Email == email);

    public async Task UpdateAsync(User user)
    {
        _context.Users.Update(user);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(User user)
    {
        _context.Users.Remove(user);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> AdminExistsAsync()
        => await _context.Users.AnyAsync(u => u.RoleId == (int)UserRole.Admin);

    public async Task<int> SaveChangesAsync()
        => await _context.SaveChangesAsync();

    public async Task<Branch?> GetBranchByIdAsync(int branchId)
    {
        return await _context.Branches.FindAsync(branchId);
    }
}




