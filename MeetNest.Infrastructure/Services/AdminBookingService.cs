using MeetNest.Application.DTOs.Admin;
using MeetNest.Application.Interfaces.Repositories;
using MeetNest.Application.Interfaces.Services;
using MeetNest.Domain.Enums;

namespace MeetNest.Infrastructure.Services;

public class AdminBookingService : IAdminBookingService
{
    private readonly IBookingRepository _bookingRepo;

    public AdminBookingService(IBookingRepository bookingRepo)
    {
        _bookingRepo = bookingRepo;
    }

    public async Task<PagedAdminBookingsDto> GetAllAsync(AdminBookingFilterDto filter)
    {
        return await _bookingRepo.GetAllAsync(filter);
    }

    public async Task<AdminBookingResponseDto?> GetByIdAsync(int id)
    {
        var booking = await _bookingRepo.GetByIdWithDetailsAsync(id);
        if (booking is null) return null;

        return new AdminBookingResponseDto
        {
            Id = booking.Id,
            RoomId = booking.RoomId,
            RoomName = booking.Room.Name,
            UserId = booking.UserId,
            EmployeeName = booking.User.FullName,
            EmployeeEmail = booking.User.Email,
            BranchId = booking.BranchId,
            BranchName = booking.Branch.Name,
            StartTime = booking.StartTime,
            EndTime = booking.EndTime,
            Status = booking.Status.ToString(),
            Priority = booking.Priority.ToString(),
            OverrideReason = booking.OverrideReason,
            ActionBy = booking.ActionBy,
            ActionAt = booking.ActionAt,
            CreatedAt = booking.CreatedAt,
            Facilities = booking.Room.RoomFacilities
                                .Where(rf => rf.Facility.IsActive)
                                .Select(rf => rf.Facility.Name)
                                .ToList()
        };
    }

    public async Task ApproveAsync(int bookingId, int adminId, BookingActionBodyDto body)
    {
        var booking = await _bookingRepo.GetByIdAsync(bookingId)
            ?? throw new Exception("Booking not found.");

        if (booking.Status != BookingStatus.Pending)
            throw new Exception("Only pending bookings can be approved.");

        // Check for conflicts unless force override
        var conflicts = (await _bookingRepo.GetApprovedBookingsForRoom(booking.RoomId))
            .Any(b => booking.StartTime < b.EndTime && booking.EndTime > b.StartTime);

        if (conflicts && !body.Force)
            throw new Exception("Time conflict exists with another approved booking. Use force=true to override.");

        booking.Status = BookingStatus.Approved;
        booking.ActionBy = adminId;
        booking.ActionAt = DateTime.UtcNow;
        booking.OverrideReason = body.Force ? (body.Reason ?? "Admin override") : body.Reason;

        await _bookingRepo.UpdateAsync(booking);
    }

    public async Task RejectAsync(int bookingId, int adminId, BookingActionBodyDto body)
    {
        var booking = await _bookingRepo.GetByIdAsync(bookingId)
            ?? throw new Exception("Booking not found.");

        if (booking.Status != BookingStatus.Pending)
            throw new Exception("Only pending bookings can be rejected.");

        booking.Status = BookingStatus.Rejected;
        booking.ActionBy = adminId;
        booking.ActionAt = DateTime.UtcNow;
        booking.OverrideReason = body.Reason;

        await _bookingRepo.UpdateAsync(booking);
    }
}
