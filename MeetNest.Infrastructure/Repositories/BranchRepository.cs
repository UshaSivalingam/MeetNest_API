using MeetNest.Application.DTOs;
using MeetNest.Application.DTOs.Branch;
using MeetNest.Application.DTOs.Filters;
using MeetNest.Application.Interfaces.Repositories;
using MeetNest.Domain.Entities;
using MeetNest.Domain.Enums;
using MeetNest.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace MeetNest.Infrastructure.Repositories;

public class BranchRepository : IBranchRepository
{
    private readonly AppDbContext _context;
    public BranchRepository(AppDbContext context) => _context = context;

    // ── Paged + filtered list ─────────────────────────────────────────────────
    public async Task<PagedResult<Branch>> GetAllAsync(BranchFilterDto filter)
    {
        var query = _context.Branches.Where(b => b.IsActive).AsQueryable();

        // Search across Name, City, Area, Country
        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var s = filter.Search.Trim().ToLower();
            query = query.Where(b =>
                b.Name.ToLower().Contains(s) ||
                b.City.ToLower().Contains(s) ||
                b.Area.ToLower().Contains(s) ||
                b.Country.ToLower().Contains(s));
        }

        if (!string.IsNullOrWhiteSpace(filter.City))
            query = query.Where(b => b.City.ToLower() == filter.City.Trim().ToLower());

        if (!string.IsNullOrWhiteSpace(filter.Country))
            query = query.Where(b => b.Country.ToLower() == filter.Country.Trim().ToLower());

        var total = await query.CountAsync();

        var items = await query
            .OrderBy(b => b.Name)
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync();

        return new PagedResult<Branch>
        {
            Items = items,
            TotalCount = total,
            Page = filter.Page,
            PageSize = filter.PageSize
        };
    }

    // ── Simple (no pagination) — for dropdowns ────────────────────────────────
    public async Task<List<Branch>> GetAllSimpleAsync()
        => await _context.Branches
            .Where(b => b.IsActive)
            .OrderBy(b => b.Name)
            .ToListAsync();

    // ── Stats (for admin dashboard) ───────────────────────────────────────────
    public async Task<List<BranchWithStatsDto>> GetAllWithStatsAsync()
    {
        return await _context.Branches
            .Where(b => b.IsActive)
            .Select(b => new BranchWithStatsDto
            {
                Id = b.Id,
                Name = b.Name,
                Area = b.Area,
                City = b.City,
                Country = b.Country,
                IsActive = b.IsActive,
                CreatedAt = b.CreatedAt,
                TotalRooms = b.Rooms.Count(r => r.IsActive),
                TotalEmployees = b.Users.Count(u => u.IsActive && u.Role.Name == UserRole.Employee),
            })
            .OrderBy(b => b.Name)
            .ToListAsync();
    }

    // ── Existing helpers ──────────────────────────────────────────────────────
    public async Task<Branch?> GetByIdAsync(int id) => await _context.Branches.FindAsync(id);
    public async Task AddAsync(Branch branch) => await _context.Branches.AddAsync(branch);
    public void Update(Branch branch) => _context.Branches.Update(branch);
    public async Task SaveChangesAsync() => await _context.SaveChangesAsync();
    public async Task<bool> ExistsAsync(int branchId)
        => await _context.Branches.AnyAsync(b => b.Id == branchId && b.IsActive);

    public async Task<bool> ExistsAsync(string name, string city)
    {
        var n = name.Trim().ToLower();
        var c = city.Trim().ToLower();
        return await _context.Branches.AnyAsync(b =>
            b.IsActive &&
            b.Name.ToLower() == n &&
            b.City.ToLower() == c);
    }
}
