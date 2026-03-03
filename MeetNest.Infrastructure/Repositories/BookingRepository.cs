using MeetNest.Application.DTOs;
using MeetNest.Application.DTOs.Admin;
using MeetNest.Application.DTOs.Filters;
using MeetNest.Application.Interfaces.Repositories;
using MeetNest.Domain.Entities;
using MeetNest.Domain.Enums;
using MeetNest.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace MeetNest.Infrastructure.Repositories;

public class BookingRepository : IBookingRepository
{
    private readonly AppDbContext _context;
    public BookingRepository(AppDbContext context) => _context = context;

    // ── Employee: my bookings — paged + filtered ──────────────────────────────
    public async Task<PagedResult<Booking>> GetEmployeeBookingsAsync(
        int userId, MyBookingFilterDto filter)
    {
        var query = _context.Bookings
            .Include(b => b.Room)
                .ThenInclude(r => r.RoomFacilities)
                    .ThenInclude(rf => rf.Facility)
            .Include(b => b.User)
            .Include(b => b.Branch)
            .Where(b => b.UserId == userId)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter.Status) &&
            Enum.TryParse<BookingStatus>(filter.Status, true, out var status))
            query = query.Where(b => b.Status == status);

        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var s = filter.Search.Trim().ToLower();
            query = query.Where(b => b.Room.Name.ToLower().Contains(s));
        }

        if (filter.From.HasValue) query = query.Where(b => b.StartTime >= filter.From.Value);
        if (filter.To.HasValue) query = query.Where(b => b.EndTime <= filter.To.Value);

        var total = await query.CountAsync();
        var items = await query
            .OrderByDescending(b => b.CreatedAt)
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync();

        return new PagedResult<Booking>
        {
            Items = items,
            TotalCount = total,
            Page = filter.Page,
            PageSize = filter.PageSize
        };
    }

    // ── Admin: all bookings — paged ───────────────────────────────────────────
    public async Task<PagedAdminBookingsDto> GetAllAsync(AdminBookingFilterDto filter)
    {
        var query = _context.Bookings
            .Include(b => b.Room)
                .ThenInclude(r => r.RoomFacilities)
                    .ThenInclude(rf => rf.Facility)
            .Include(b => b.User)
            .Include(b => b.Branch)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter.Status) &&
            Enum.TryParse<BookingStatus>(filter.Status, true, out var parsedStatus))
            query = query.Where(b => b.Status == parsedStatus);

        if (filter.BranchId.HasValue) query = query.Where(b => b.BranchId == filter.BranchId.Value);
        if (filter.From.HasValue) query = query.Where(b => b.StartTime >= filter.From.Value);
        if (filter.To.HasValue) query = query.Where(b => b.EndTime <= filter.To.Value);

        var totalCount = await query.CountAsync();
        var items = await query
            .OrderByDescending(b => b.Priority)   // High priority first
            .ThenByDescending(b => b.CreatedAt)
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .Select(b => MapToAdminDto(b))
            .ToListAsync();

        return new PagedAdminBookingsDto
        {
            Items = items,
            TotalCount = totalCount,
            Page = filter.Page,
            PageSize = filter.PageSize
        };
    }

    // ── FIX: only block if an APPROVED booking exists for same slot ───────────
    // Previously blocked on any non-cancelled booking — this prevented multiple
    // employees from submitting for the same slot. Now only Approved blocks.
    public async Task<bool> IsRoomBookedAsync(int roomId, DateTime startTime, DateTime endTime)
        => await _context.Bookings.AnyAsync(b =>
            b.RoomId == roomId &&
            b.Status == BookingStatus.Approved &&   // ✅ Only Approved blocks new bookings
            startTime < b.EndTime &&
            endTime > b.StartTime);

    // ── NEW: get other pending bookings that clash with same room/slot ────────
    public async Task<List<Booking>> GetConflictingPendingBookings(
        int roomId, DateTime startTime, DateTime endTime, int excludeBookingId)
        => await _context.Bookings
            .Include(b => b.User)
            .Where(b =>
                b.RoomId == roomId &&
                b.Id != excludeBookingId &&
                b.Status == BookingStatus.Pending &&
                startTime < b.EndTime &&
                endTime > b.StartTime)
            .OrderByDescending(b => b.Priority)   // High priority first
            .ThenBy(b => b.CreatedAt)             // Earlier request first on tie
            .ToListAsync();

    // ── Existing helpers ──────────────────────────────────────────────────────
    public async Task AddAsync(Booking booking)
    {
        await _context.Bookings.AddAsync(booking);
        await _context.SaveChangesAsync();
    }

    public async Task<Booking?> GetByIdAsync(int id)
        => await _context.Bookings
            .Include(b => b.Room).ThenInclude(r => r.RoomFacilities).ThenInclude(rf => rf.Facility)
            .Include(b => b.User)
            .FirstOrDefaultAsync(b => b.Id == id);

    public async Task<Booking?> GetByIdWithDetailsAsync(int id)
        => await _context.Bookings
            .Include(b => b.Room).ThenInclude(r => r.RoomFacilities).ThenInclude(rf => rf.Facility)
            .Include(b => b.User).ThenInclude(u => u.Branch)
            .Include(b => b.Branch)
            .FirstOrDefaultAsync(b => b.Id == id);

    public async Task<List<Booking>> GetRoomBookingsAsync(int roomId)
        => await _context.Bookings
            .Where(b => b.RoomId == roomId && b.Status == BookingStatus.Approved)
            .ToListAsync();

    public async Task UpdateAsync(Booking booking)
    {
        _context.Bookings.Update(booking);
        await _context.SaveChangesAsync();
    }

    public async Task<List<Booking>> GetApprovedBookingsForRoom(int roomId)
        => await _context.Bookings
            .Where(b => b.RoomId == roomId && b.Status == BookingStatus.Approved)
            .ToListAsync();

    public async Task<int> CountByStatusAsync(string status)
    {
        if (!Enum.TryParse<BookingStatus>(status, true, out var s)) return 0;
        return await _context.Bookings.CountAsync(b => b.Status == s);
    }

    public async Task<int> CountTotalAsync() => await _context.Bookings.CountAsync();

    public async Task<List<Booking>> GetRecentAsync(int count)
        => await _context.Bookings
            .Include(b => b.Room).Include(b => b.User).Include(b => b.Branch)
            .OrderByDescending(b => b.CreatedAt)
            .Take(count)
            .ToListAsync();

    private static AdminBookingResponseDto MapToAdminDto(Booking b) => new()
    {
        Id = b.Id,
        RoomId = b.RoomId,
        RoomName = b.Room.Name,
        UserId = b.UserId,
        EmployeeName = b.User.FullName,
        EmployeeEmail = b.User.Email,
        BranchId = b.BranchId,
        BranchName = b.Branch.Name,
        StartTime = b.StartTime,
        EndTime = b.EndTime,
        Status = b.Status.ToString(),
        Priority = b.Priority.ToString(),
        OverrideReason = b.OverrideReason,
        Notes = b.Notes,
        ActionBy = b.ActionBy,
        ActionAt = b.ActionAt,
        CreatedAt = b.CreatedAt,
        Facilities = b.Room.RoomFacilities
                           .Where(rf => rf.Facility.IsActive)
                           .Select(rf => rf.Facility.Name)
                           .ToList()
        // ConflictingBookings populated separately in GetByIdAsync (service layer)
    };
}