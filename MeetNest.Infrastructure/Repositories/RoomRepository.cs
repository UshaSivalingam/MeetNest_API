using MeetNest.Application.DTOs;
using MeetNest.Application.DTOs.Filters;
using MeetNest.Application.Interfaces.Repositories;
using MeetNest.Domain.Entities;
using MeetNest.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace MeetNest.Infrastructure.Repositories;

public class RoomRepository : IRoomRepository
{
    private readonly AppDbContext _context;
    public RoomRepository(AppDbContext context) => _context = context;

    // ── Base query with includes ──────────────────────────────────────────────
    private IQueryable<Room> BaseQuery() =>
        _context.Rooms
            .Include(r => r.Branch)
            .Include(r => r.RoomFacilities)
                .ThenInclude(rf => rf.Facility)
            .Where(r => r.IsActive);

    private static IQueryable<Room> ApplySearch(IQueryable<Room> query, RoomFilterDto filter)
    {
        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var s = filter.Search.Trim().ToLower();
            query = query.Where(r => r.Name.ToLower().Contains(s));
        }
        if (filter.MinCap.HasValue) query = query.Where(r => r.Capacity >= filter.MinCap.Value);
        if (filter.MaxCap.HasValue) query = query.Where(r => r.Capacity <= filter.MaxCap.Value);
        return query;
    }

    private static async Task<PagedResult<Room>> ToPagedAsync(
        IQueryable<Room> query, int page, int pageSize)
    {
        var total = await query.CountAsync();
        var items = await query
            .OrderBy(r => r.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResult<Room>
        {
            Items = items,
            TotalCount = total,
            Page = page,
            PageSize = pageSize
        };
    }

    // ── Paged: all rooms (admin) ──────────────────────────────────────────────
    public async Task<PagedResult<Room>> GetAllAsync(RoomFilterDto filter)
    {
        var query = BaseQuery();
        if (filter.BranchId.HasValue) query = query.Where(r => r.BranchId == filter.BranchId.Value);
        query = ApplySearch(query, filter);
        return await ToPagedAsync(query, filter.Page, filter.PageSize);
    }

    // ── Paged: rooms by branch (admin) ────────────────────────────────────────
    public async Task<PagedResult<Room>> GetByBranchIdAsync(int branchId, RoomFilterDto filter)
    {
        var query = BaseQuery().Where(r => r.BranchId == branchId);
        query = ApplySearch(query, filter);
        return await ToPagedAsync(query, filter.Page, filter.PageSize);
    }

    // ── Simple list: employee room picker (no pagination) ─────────────────────
    public async Task<List<Room>> GetByBranchIdSimpleAsync(int branchId)
        => await _context.Rooms
            .Include(r => r.Branch)
            .Include(r => r.RoomFacilities).ThenInclude(rf => rf.Facility)
            .Where(r => r.BranchId == branchId && r.IsActive)
            .OrderBy(r => r.Name)
            .ToListAsync();

    // ── Existing helpers ──────────────────────────────────────────────────────
    public async Task<Room?> GetByIdAsync(int id) => await _context.Rooms.FindAsync(id);
    public async Task<Room?> GetByIdWithFacilitiesAsync(int id)
        => await _context.Rooms
            .Include(r => r.Branch)
            .Include(r => r.RoomFacilities).ThenInclude(rf => rf.Facility)
            .FirstOrDefaultAsync(r => r.Id == id);

    public async Task AddAsync(Room room)
    {
        await _context.Rooms.AddAsync(room);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Room room)
    {
        _context.Rooms.Update(room);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Room room)
    {
        room.IsActive = false;
        room.UpdatedAt = DateTime.UtcNow;
        _context.Rooms.Update(room);
        await _context.SaveChangesAsync();
    }
}
