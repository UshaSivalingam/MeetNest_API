using MeetNest.Application.DTOs.Admin;
using MeetNest.Application.Interfaces.Repositories;
using MeetNest.Application.Interfaces.Services;
using MeetNest.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace MeetNest.Infrastructure.Services;

public class AdminBookingService : IAdminBookingService
{
    private readonly IBookingRepository _bookingRepo;
    private readonly IUserRepository _userRepo;
    private readonly INotificationService _notifService;
    private readonly IEmailService _emailService;
    private readonly ILogger<AdminBookingService> _logger;

    public AdminBookingService(
        IBookingRepository bookingRepo,
        IUserRepository userRepo,
        INotificationService notifService,
        IEmailService emailService,
        ILogger<AdminBookingService> logger)
    {
        _bookingRepo = bookingRepo;
        _userRepo = userRepo;
        _notifService = notifService;
        _emailService = emailService;
        _logger = logger;
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

    // ── Approve ───────────────────────────────────────────────────
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

        // ── Schedule bell reminder ────────────────────────────────
        try
        {
            await _notifService.ScheduleMeetingEndReminderAsync(
                bookingId: booking.Id,
                adminId: adminId,
                roomName: booking.Room.Name,
                endTime: booking.EndTime);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to schedule bell reminder for booking {Id}", bookingId);
        }

        // ── Load ALL user data NOW while DbContext is still alive ─
        // Task.Run runs after the HTTP request ends — DbContext is
        // disposed by then. Never call _userRepo inside Task.Run.
        var approvedEmployee = await _userRepo.GetByIdAsync(booking.UserId);

        // Load each auto-rejected employee upfront too
        var loserData = new List<(string Email, string FullName, DateTime Start, DateTime End)>();
        foreach (var conflict in pendingConflicts)
        {
            var loser = await _userRepo.GetByIdAsync(conflict.UserId);
            if (loser is not null)
                loserData.Add((loser.Email, loser.FullName, conflict.StartTime, conflict.EndTime));
        }

        // ── Capture plain values for Task.Run ─────────────────────
        var roomName = booking.Room.Name;
        var branchName = booking.Branch?.Name ?? string.Empty;
        var startTime = booking.StartTime;
        var endTime = booking.EndTime;

        _ = Task.Run(async () =>
        {
            try
            {
                // Email approved employee
                if (approvedEmployee is not null)
                {
                    await _emailService.SendBookingApprovedAsync(
                        approvedEmployee.Email, approvedEmployee.FullName,
                        roomName, branchName, startTime, endTime);
                    _logger.LogInformation("Approval email sent to {Email}", approvedEmployee.Email);
                }

                // Email each auto-rejected employee
                foreach (var (email, fullName, cStart, cEnd) in loserData)
                {
                    await _emailService.SendBookingAutoRejectedAsync(
                        email, fullName, roomName, cStart, cEnd);
                    _logger.LogInformation("Auto-reject email sent to {Email}", email);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Email failed in ApproveAsync for booking {Id}: {Message}",
                    bookingId, ex.Message);
            }
        });
    }

    // ── Reject ────────────────────────────────────────────────────
    public async Task RejectAsync(int bookingId, int adminId, BookingActionBodyDto body)
    {
        var booking = await _bookingRepo.GetByIdWithDetailsAsync(bookingId)
            ?? throw new Exception("Booking not found.");

        if (booking.Status != BookingStatus.Pending)
            throw new Exception("Only pending bookings can be rejected.");

        booking.Status = BookingStatus.Rejected;
        booking.ActionBy = adminId;
        booking.ActionAt = DateTime.UtcNow;
        booking.OverrideReason = body.Reason;
        booking.UpdatedAt = DateTime.UtcNow;
        await _bookingRepo.UpdateAsync(booking);

        // ── Load user data NOW while DbContext is still alive ─────
        var employee = await _userRepo.GetByIdAsync(booking.UserId);

        // Capture plain values
        var roomName = booking.Room?.Name ?? "your room";
        var reason = body.Reason;

        _ = Task.Run(async () =>
        {
            try
            {
                if (employee is not null)
                {
                    await _emailService.SendBookingRejectedAsync(
                        employee.Email, employee.FullName, roomName, reason);
                    _logger.LogInformation("Rejection email sent to {Email}", employee.Email);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Email failed in RejectAsync for booking {Id}: {Message}",
                    bookingId, ex.Message);
            }
        });
    }
}