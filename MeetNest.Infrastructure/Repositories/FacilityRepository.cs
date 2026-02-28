using MeetNest.Application.DTOs;
using MeetNest.Application.DTOs.Filters;
using MeetNest.Application.Interfaces.Repositories;
using MeetNest.Domain.Entities;
using MeetNest.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace MeetNest.Infrastructure.Repositories;

public class FacilityRepository : IFacilityRepository
{
    private readonly AppDbContext _context;
    public FacilityRepository(AppDbContext context) => _context = context;

    // ── Paged + filtered ──────────────────────────────────────────────────────
    public async Task<PagedResult<Facility>> GetAllAsync(FacilityFilterDto filter)
    {
        var query = _context.Facilities.Where(f => f.IsActive).AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var s = filter.Search.Trim().ToLower();
            query = query.Where(f =>
                f.Name.ToLower().Contains(s) ||
                f.Description.ToLower().Contains(s));
        }

        var total = await query.CountAsync();
        var items = await query
            .OrderBy(f => f.Name)
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync();

        return new PagedResult<Facility>
        {
            Items = items,
            TotalCount = total,
            Page = filter.Page,
            PageSize = filter.PageSize
        };
    }

    // ── Simple list for dropdowns ─────────────────────────────────────────────
    public async Task<List<Facility>> GetAllSimpleAsync()
        => await _context.Facilities
            .Where(f => f.IsActive)
            .OrderBy(f => f.Name)
            .ToListAsync();

    // ── Existing helpers ──────────────────────────────────────────────────────
    public async Task AddAsync(Facility facility) => await _context.Facilities.AddAsync(facility);
    public async Task<Facility?> GetByIdAsync(int id) => await _context.Facilities.FindAsync(id);
    public async Task<bool> ExistsByNameAsync(string name) => await _context.Facilities.AnyAsync(f => f.Name == name && f.IsActive);
    public void Update(Facility facility) => _context.Facilities.Update(facility);
    public async Task SaveChangesAsync() => await _context.SaveChangesAsync();

    public async Task DeleteAsync(Facility facility)
    {
        facility.IsActive = false;
        _context.Facilities.Update(facility);
        await Task.CompletedTask;
    }
}
