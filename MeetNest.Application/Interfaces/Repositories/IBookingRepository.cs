using MeetNest.Application.DTOs;
using MeetNest.Application.DTOs.Admin;
using MeetNest.Application.DTOs.Filters;
using MeetNest.Domain.Entities;

namespace MeetNest.Application.Interfaces.Repositories;

public interface IBookingRepository
{
    Task AddAsync(Booking booking);
    Task<Booking?> GetByIdAsync(int id);
    Task<List<Booking>> GetRoomBookingsAsync(int roomId);
    Task UpdateAsync(Booking booking);
    Task<List<Booking>> GetApprovedBookingsForRoom(int roomId);
    Task<bool> IsRoomBookedAsync(int roomId, DateTime startTime, DateTime endTime);
    Task<Booking?> GetByIdWithDetailsAsync(int id);
    Task<int> CountByStatusAsync(string status);
    Task<int> CountTotalAsync();
    Task<List<Booking>> GetRecentAsync(int count);

    // ── Paged queries ─────────────────────────────────────────────────────────
    Task<PagedAdminBookingsDto> GetAllAsync(AdminBookingFilterDto filter);
    Task<PagedResult<Booking>> GetEmployeeBookingsAsync(int userId, MyBookingFilterDto filter);

    // ──get other pending bookings that clash with a given room/slot ─────
    Task<List<Booking>> GetConflictingPendingBookings(
        int roomId, DateTime startTime, DateTime endTime, int excludeBookingId);
}