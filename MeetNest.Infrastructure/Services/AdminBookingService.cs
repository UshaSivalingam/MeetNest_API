// MeetNest.Infrastructure/Services/AdminBookingService.cs
// ── REPLACE your existing file entirely ──

using MeetNest.Application.DTOs.Admin;
using MeetNest.Application.Interfaces.Repositories;
using MeetNest.Application.Interfaces.Services;
using MeetNest.Domain.Enums;

namespace MeetNest.Infrastructure.Services;

public class AdminBookingService : IAdminBookingService
{
    private readonly IBookingRepository _bookingRepo;
    private readonly INotificationService _notifService;

    public AdminBookingService(
        IBookingRepository bookingRepo,
        INotificationService notifService)   // ← NEW dependency
    {
        _bookingRepo = bookingRepo;
        _notifService = notifService;
    }

    public async Task<PagedAdminBookingsDto> GetAllAsync(AdminBookingFilterDto filter)
        => await _bookingRepo.GetAllAsync(filter);

    public async Task<AdminBookingResponseDto?> GetByIdAsync(int id)
    {
        var booking = await _bookingRepo.GetByIdWithDetailsAsync(id);
        if (booking is null) return null;

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

    // ── Approve — now schedules an end-time reminder ──────────────
    public async Task ApproveAsync(int bookingId, int adminId, BookingActionBodyDto body)
    {
        var booking = await _bookingRepo.GetByIdWithDetailsAsync(bookingId)
            ?? throw new Exception("Booking not found.");

        if (booking.Status != BookingStatus.Pending)
            throw new Exception("Only pending bookings can be approved.");

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

        // Auto-reject conflicting pending bookings
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

        // ── Schedule end-time reminder for the approving admin ────
        // Fire-and-forget — don't let notification failure break approval
        try
        {
            await _notifService.ScheduleMeetingEndReminderAsync(
                bookingId: booking.Id,
                adminId: adminId,
                roomName: booking.Room.Name,
                endTime: booking.EndTime);
        }
        catch
        {
            // Swallow — approval itself succeeded
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