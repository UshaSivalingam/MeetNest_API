using MeetNest.Application.DTOs;
using MeetNest.Application.DTOs.Booking;
using MeetNest.Application.DTOs.Filters;

namespace MeetNest.Application.Interfaces.Services;

public interface IBookingService
{
    Task<int> CreateAsync(int employeeId, CreateBookingDto dto);
    Task<PagedResult<BookingResponseDto>> GetMyBookingsAsync(int employeeId, MyBookingFilterDto filter);
    Task CancelAsync(int bookingId, int employeeId);
    Task ApproveAsync(int bookingId, int adminId, bool force = false, string? reason = null);
    Task RejectAsync(int bookingId, int adminId, string? reason = null);
}
