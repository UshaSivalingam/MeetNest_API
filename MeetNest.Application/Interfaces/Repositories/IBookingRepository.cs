
using MeetNest.Application.DTOs;
using MeetNest.Application.DTOs.Admin;
using MeetNest.Application.DTOs.Filters;
using MeetNest.Domain.Entities;

namespace MeetNest.Application.Interfaces.Repositories;

public interface IBookingRepository
{
    // ── Existing (unchanged) ──────────────────────────────────────────────────
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

    // ── Updated: admin bookings — already paged (unchanged) ───────────────────
    Task<PagedAdminBookingsDto> GetAllAsync(AdminBookingFilterDto filter);

    // ── Updated: employee my-bookings — now paged + filtered ──────────────────
    Task<PagedResult<Booking>> GetEmployeeBookingsAsync(int userId, MyBookingFilterDto filter);
}
