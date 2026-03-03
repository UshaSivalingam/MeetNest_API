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
        => await _bookingRepo.GetAllAsync(filter);

    public async Task<AdminBookingResponseDto?> GetByIdAsync(int id)
    {
        var booking = await _bookingRepo.GetByIdWithDetailsAsync(id);
        if (booking is null) return null;

        // Fetch other pending bookings for the same room/slot so admin can
        // compare priorities before deciding
        var conflicts = await _bookingRepo.GetConflictingPendingBookings(
            booking.RoomId, booking.StartTime, booking.EndTime, booking.Id);

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
            Notes = booking.Notes,
            ActionBy = booking.ActionBy,
            ActionAt = booking.ActionAt,
            CreatedAt = booking.CreatedAt,
            Facilities = booking.Room.RoomFacilities
                                    .Where(rf => rf.Facility.IsActive)
                                    .Select(rf => rf.Facility.Name)
                                    .ToList(),
            // ✅ Conflicting pending bookings for admin to see before deciding
            ConflictingBookings = conflicts.Select(c => new ConflictingBookingDto
            {
                Id = c.Id,
                EmployeeName = c.User.FullName,
                EmployeeEmail = c.User.Email,
                Priority = c.Priority.ToString(),
                Notes = c.Notes,
                StartTime = c.StartTime,
                EndTime = c.EndTime,
                CreatedAt = c.CreatedAt,
            }).ToList()
        };
    }

    public async Task ApproveAsync(int bookingId, int adminId, BookingActionBodyDto body)
    {
        var booking = await _bookingRepo.GetByIdAsync(bookingId)
            ?? throw new Exception("Booking not found.");

        if (booking.Status != BookingStatus.Pending)
            throw new Exception("Only pending bookings can be approved.");

        // Check for conflicts with already-approved bookings
        var approvedConflicts = (await _bookingRepo.GetApprovedBookingsForRoom(booking.RoomId))
            .Any(b => booking.StartTime < b.EndTime && booking.EndTime > b.StartTime);

        if (approvedConflicts && !body.Force)
            throw new Exception("Time conflict with an already approved booking. Use force=true to override.");

        booking.Status = BookingStatus.Approved;
        booking.ActionBy = adminId;
        booking.ActionAt = DateTime.UtcNow;
        booking.OverrideReason = body.Force ? (body.Reason ?? "Admin override") : body.Reason;
        booking.UpdatedAt = DateTime.UtcNow;
        await _bookingRepo.UpdateAsync(booking);

        // ✅ Auto-reject all other pending bookings for the same room/slot
        // with a clear reason so employees know why they were rejected
        var pendingConflicts = await _bookingRepo.GetConflictingPendingBookings(
            booking.RoomId, booking.StartTime, booking.EndTime, bookingId);

        foreach (var conflict in pendingConflicts)
        {
            conflict.Status = BookingStatus.Rejected;
            conflict.ActionBy = adminId;
            conflict.ActionAt = DateTime.UtcNow;
            conflict.OverrideReason = "Another booking was approved for this time slot.";
            conflict.UpdatedAt = DateTime.UtcNow;
            await _bookingRepo.UpdateAsync(conflict);
        }
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
        booking.UpdatedAt = DateTime.UtcNow;
        await _bookingRepo.UpdateAsync(booking);
    }
}