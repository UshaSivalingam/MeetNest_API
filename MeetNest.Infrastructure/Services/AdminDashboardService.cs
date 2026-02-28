using MeetNest.Application.DTOs.Admin;
using MeetNest.Application.Interfaces.Repositories;
using MeetNest.Application.Interfaces.Services;
using MeetNest.Domain.Enums;
using MeetNest.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace MeetNest.Infrastructure.Services;

public class AdminDashboardService : IAdminDashboardService
{
    private readonly AppDbContext _db;

    public AdminDashboardService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<DashboardStatsDto> GetStatsAsync()
    {
        // ── Stat card counts ──────────────────────────────────────────────────
        var totalBranches = await _db.Branches.CountAsync(b => b.IsActive);
        var totalRooms = await _db.Rooms.CountAsync(r => r.IsActive);
        var totalEmployees = await _db.Users
                                       .CountAsync(u => u.IsActive && u.Role.Name == UserRole.Employee);
        var totalBookings = await _db.Bookings.CountAsync();

        var pendingCount = await _db.Bookings.CountAsync(b => b.Status == BookingStatus.Pending);
        var approvedCount = await _db.Bookings.CountAsync(b => b.Status == BookingStatus.Approved);
        var rejectedCount = await _db.Bookings.CountAsync(b => b.Status == BookingStatus.Rejected);
        var cancelledCount = await _db.Bookings.CountAsync(b => b.Status == BookingStatus.Cancelled);

        // ── Recent 10 bookings for dashboard table ────────────────────────────
        var recentBookings = await _db.Bookings
            .Include(b => b.Room)
            .Include(b => b.User)
            .Include(b => b.Branch)
            .OrderByDescending(b => b.CreatedAt)
            .Take(10)
            .Select(b => new RecentBookingDto
            {
                Id = b.Id,
                EmployeeName = b.User.FullName,
                RoomName = b.Room.Name,
                BranchName = b.Branch.Name,
                StartTime = b.StartTime,
                EndTime = b.EndTime,
                Status = b.Status.ToString(),
                Priority = b.Priority.ToString()
            })
            .ToListAsync();

        // ── Bookings per branch for chart ─────────────────────────────────────
        var bookingsPerBranch = await _db.Branches
            .Where(br => br.IsActive)
            .Select(br => new BranchBookingStatDto
            {
                BranchName = br.Name,
                TotalBookings = br.Rooms
                                     .SelectMany(r => r.Bookings)
                                     .Count(),
                PendingBookings = br.Rooms
                                     .SelectMany(r => r.Bookings)
                                     .Count(b => b.Status == BookingStatus.Pending),
                ApprovedBookings = br.Rooms
                                     .SelectMany(r => r.Bookings)
                                     .Count(b => b.Status == BookingStatus.Approved)
            })
            .OrderBy(x => x.BranchName)
            .ToListAsync();

        return new DashboardStatsDto
        {
            TotalBranches = totalBranches,
            TotalRooms = totalRooms,
            TotalEmployees = totalEmployees,
            TotalBookings = totalBookings,
            PendingBookings = pendingCount,
            ApprovedBookings = approvedCount,
            RejectedBookings = rejectedCount,
            CancelledBookings = cancelledCount,
            RecentBookings = recentBookings,
            BookingsPerBranch = bookingsPerBranch
        };
    }
}
