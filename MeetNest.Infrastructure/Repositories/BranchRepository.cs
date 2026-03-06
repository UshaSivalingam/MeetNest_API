// MeetNest.Infrastructure/Repositories/BranchRepository.cs

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

    public BranchRepository(AppDbContext context)
    {
        _context = context;
    }

    // ── Get by ID ─────────────────────────────────────────────────
    public async Task<Branch?> GetByIdAsync(int id)
        => await _context.Branches.FirstOrDefaultAsync(b => b.Id == id);

    // ── Add ───────────────────────────────────────────────────────
    public async Task AddAsync(Branch branch)
        => await _context.Branches.AddAsync(branch);

    // ── Update ────────────────────────────────────────────────────
    public void Update(Branch branch)
        => _context.Branches.Update(branch);

    // ── Save ──────────────────────────────────────────────────────
    public async Task SaveChangesAsync()
        => await _context.SaveChangesAsync();

    // ── Exists by ID ──────────────────────────────────────────────
    public async Task<bool> ExistsAsync(int branchId)
        => await _context.Branches.AnyAsync(b => b.Id == branchId && b.IsActive);

    // ── Exists by Name + City ─────────────────────────────────────
    public async Task<bool> ExistsAsync(string name, string city)
        => await _context.Branches.AnyAsync(b =>
            b.Name.ToLower() == name.ToLower() &&
            b.City.ToLower() == city.ToLower() &&
            b.IsActive);

    // ── GetAllWithStatsAsync (kept for any other callers) ─────────
    public async Task<List<BranchWithStatsDto>> GetAllWithStatsAsync()
        => await BuildStatsQuery(_context.Branches.Where(b => b.IsActive))
            .OrderBy(b => b.Name)
            .ToListAsync();

    // ── GetAllSimpleAsync — lightweight list for dropdowns ────────
    public async Task<List<Branch>> GetAllSimpleAsync()
        => await _context.Branches
            .Where(b => b.IsActive)
            .OrderBy(b => b.Name)
            .ToListAsync();

    // ── GetAllAsync — PAGED + FILTERED, returns stats ─────────────
    //
    //  FIX 1 — Returns PagedResult<BranchWithStatsDto> instead of
    //           PagedResult<Branch>, so totalRooms / totalEmployees /
    //           totalBookings are populated correctly.
    //
    //  FIX 2 — Pagination is done entirely here; the frontend passes
    //           page & pageSize and gets back ONLY the requested slice,
    //           solving the "7 branches → 6 + 1" problem caused by the
    //           frontend re-slicing an already-paginated list.
    //
    public async Task<PagedResult<BranchWithStatsDto>> GetAllAsync(BranchFilterDto filter)
    {
        // ── 1. Base query (active only) ───────────────────────────
        var query = _context.Branches.Where(b => b.IsActive);

        // ── 2. Search filter ──────────────────────────────────────
        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var q = filter.Search.Trim().ToLower();
            query = query.Where(b =>
                b.Name.ToLower().Contains(q) ||
                b.City.ToLower().Contains(q) ||
                b.Country.ToLower().Contains(q) ||
                b.Area.ToLower().Contains(q));
        }

        // ── 3. Country filter ─────────────────────────────────────
        if (!string.IsNullOrWhiteSpace(filter.Country))
            query = query.Where(b => b.Country == filter.Country);

        // ── 4. City filter ────────────────────────────────────────
        if (!string.IsNullOrWhiteSpace(filter.City))
            query = query.Where(b => b.City == filter.City);

        // ── 5. Total count BEFORE paging ─────────────────────────
        var totalCount = await query.CountAsync();

        // ── 6. Sort ───────────────────────────────────────────────
        query = (filter.SortBy?.ToLower()) switch
        {
            "city" => query.OrderBy(b => b.City),
            "country" => query.OrderBy(b => b.Country),
            "rooms_desc" => query.OrderByDescending(b => b.Rooms.Count(r => r.IsActive)),
            "rooms_asc" => query.OrderBy(b => b.Rooms.Count(r => r.IsActive)),
            _ => query.OrderBy(b => b.Name),   // default: name A-Z
        };

        // ── 7. Page + project to DTO (single round-trip) ──────────
        var page = filter.Page < 1 ? 1 : filter.Page;
        var pageSize = filter.PageSize < 1 ? 10 : filter.PageSize;

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(b => new BranchWithStatsDto
            {
                Id = b.Id,
                Name = b.Name,
                Area = b.Area,
                City = b.City,
                Country = b.Country,
                IsActive = b.IsActive,
                CreatedAt = b.CreatedAt,

                // Count only active rooms
                TotalRooms = b.Rooms.Count(r => r.IsActive),

                // Count only active users linked to this branch
                TotalEmployees = b.Users.Count(u => u.IsActive),

                // Count bookings that are in Approved status across all rooms
                TotalBookings = b.Rooms
                    .SelectMany(r => r.Bookings)
                    .Count(bk => bk.Status == BookingStatus.Approved),
            })
            .ToListAsync();

        return new PagedResult<BranchWithStatsDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize,
        };
    }

    // ── Shared projection helper (reused by GetAllWithStatsAsync) ─
    private static IQueryable<BranchWithStatsDto> BuildStatsQuery(
        IQueryable<Branch> source)
        => source.Select(b => new BranchWithStatsDto
        {
            Id = b.Id,
            Name = b.Name,
            Area = b.Area,
            City = b.City,
            Country = b.Country,
            IsActive = b.IsActive,
            CreatedAt = b.CreatedAt,
            TotalRooms = b.Rooms.Count(r => r.IsActive),
            TotalEmployees = b.Users.Count(u => u.IsActive),
            TotalBookings = b.Rooms
                .SelectMany(r => r.Bookings)
                .Count(bk => bk.Status == BookingStatus.Approved),
        });
}