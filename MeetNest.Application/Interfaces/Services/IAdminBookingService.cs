using MeetNest.Application.DTOs.Admin;

namespace MeetNest.Application.Interfaces.Services;

public interface IAdminBookingService
{
    Task<PagedAdminBookingsDto> GetAllAsync(AdminBookingFilterDto filter);
    Task<AdminBookingResponseDto?> GetByIdAsync(int id);
    Task ApproveAsync(int bookingId, int adminId, BookingActionBodyDto body);
    Task RejectAsync(int bookingId, int adminId, BookingActionBodyDto body);
}
