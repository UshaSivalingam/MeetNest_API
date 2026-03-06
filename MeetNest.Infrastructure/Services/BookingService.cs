using MeetNest.Application.DTOs;
using MeetNest.Application.DTOs.Booking;
using MeetNest.Application.DTOs.Filters;
using MeetNest.Application.Interfaces.Repositories;
using MeetNest.Application.Interfaces.Services;
using MeetNest.Domain.Entities;
using MeetNest.Domain.Enums;

public class BookingService : IBookingService
{
    private readonly IBookingRepository _bookingRepo;
    private readonly IRoomRepository _roomRepo;
    private readonly IUserRepository _userRepo;

    public BookingService(
        IBookingRepository bookingRepo,
        IRoomRepository roomRepo,
        IUserRepository userRepo)
    {
        _bookingRepo = bookingRepo;
        _roomRepo = roomRepo;
        _userRepo = userRepo;
    }

    public async Task<int> CreateAsync(int userId, CreateBookingDto dto)
    {
        // ── 1. Normalise to UTC ───────────────────────────────────────
        var startUtc = dto.StartTime.Kind == DateTimeKind.Utc
            ? dto.StartTime
            : DateTime.SpecifyKind(dto.StartTime, DateTimeKind.Utc);

        var endUtc = dto.EndTime.Kind == DateTimeKind.Utc
            ? dto.EndTime
            : DateTime.SpecifyKind(dto.EndTime, DateTimeKind.Utc);

        ValidateTimeline(startUtc, endUtc);

        // ── 2. Load room + user ───────────────────────────────────────
        var room = await _roomRepo.GetByIdAsync(dto.RoomId)
            ?? throw new Exception("Room not found.");

        var user = await _userRepo.GetByIdAsync(userId)
            ?? throw new Exception("User not found.");

        if (room.BranchId != user.BranchId)
            throw new Exception("Cannot book a room from another branch.");

        // ── 3. Conflict check — always block on Approved overlaps ─────
        // (Pending overlaps are allowed; admin decides who wins)
        if (await _bookingRepo.IsRoomBookedAsync(dto.RoomId, startUtc, endUtc))
            throw new Exception("This room already has an approved booking for the selected time slot.");

        // ── 4. Decide initial status based on ApprovalRequired ────────
        //
        //   ApprovalRequired = true  → status stays Pending (admin must act)
        //   ApprovalRequired = false → status set to Approved immediately
        //
        var initialStatus = room.ApprovalRequired
            ? BookingStatus.Pending
            : BookingStatus.Approved;

        // ── 5. Persist ────────────────────────────────────────────────
        var booking = new Booking
        {
            RoomId = dto.RoomId,
            UserId = userId,
            BranchId = room.BranchId,
            StartTime = startUtc,
            EndTime = endUtc,
            Priority = dto.Priority,
            Notes = dto.Notes,
            Status = initialStatus,

            // If auto-approved, record system as the actor
            ActionBy = initialStatus == BookingStatus.Approved ? userId : null,
            ActionAt = initialStatus == BookingStatus.Approved ? DateTime.UtcNow : null,
        };

        await _bookingRepo.AddAsync(booking);
        return booking.Id;
    }

    public async Task<PagedResult<BookingResponseDto>> GetMyBookingsAsync(
    int employeeId, MyBookingFilterDto filter)
    {
        var paged = await _bookingRepo.GetEmployeeBookingsAsync(employeeId, filter);

        return new PagedResult<BookingResponseDto>
        {
            Items = paged.Items.Select(b => new BookingResponseDto
            {
                Id = b.Id,
                RoomName = b.Room.Name,
                BranchName = b.Branch?.Name ?? string.Empty,
                EmployeeName = b.User.FullName,
                StartTime = b.StartTime,
                EndTime = b.EndTime,
                Status = b.Status.ToString(),
                Priority = b.Priority.ToString(),
                OverrideReason = b.OverrideReason,
                Notes = b.Notes,
                CreatedAt = b.CreatedAt,
                ActionAt = b.ActionAt,
                Facilities = b.Room.RoomFacilities
                                     .Where(rf => rf.Facility.IsActive)
                                     .Select(rf => rf.Facility.Name)
                                     .ToList()
            }).ToList(),
            TotalCount = paged.TotalCount,
            Page = paged.Page,
            PageSize = paged.PageSize
        };
    }
    /*public async Task<PagedResult<BookingResponseDto>> GetMyBookingsAsync(
        int employeeId, MyBookingFilterDto filter)
    {
        var paged = await _bookingRepo.GetEmployeeBookingsAsync(employeeId, filter);
        return new PagedResult<BookingResponseDto>
        {
            Items = paged.Items.Select(b => new BookingResponseDto
            {
                Id = b.Id,
                RoomName = b.Room.Name,
                BranchName = b.Branch?.Name ?? string.Empty,
                EmployeeName = b.User.FullName,
                StartTime = b.StartTime,
                EndTime = b.EndTime,
                Status = b.Status.ToString(),
                Priority = b.Priority.ToString(),
                OverrideReason = b.OverrideReason,
                Notes = b.Notes,
                CreatedAt = b.CreatedAt,
                ActionAt = b.ActionAt,
                Facilities = b.Room.RoomFacilities
                                     .Where(rf => rf.Facility.IsActive)
                                     .Select(rf => rf.Facility.Name)
                                     .ToList()
            }).ToList(),
            TotalCount = paged.TotalCount,
            Page = paged.Page,
            PageSize = paged.PageSize
        };
    }*/

    public async Task CancelAsync(int bookingId, int userId)
    {
        var booking = await _bookingRepo.GetByIdAsync(bookingId)
            ?? throw new Exception("Booking not found.");

        if (booking.UserId != userId)
            throw new Exception("Unauthorized.");

        if (booking.Status == BookingStatus.Approved && booking.StartTime <= DateTime.UtcNow)
            throw new Exception("Cannot cancel an ongoing booking.");

        if (booking.Status == BookingStatus.Rejected || booking.Status == BookingStatus.Cancelled)
            throw new Exception("Booking is already closed.");

        booking.Status = BookingStatus.Cancelled;
        booking.UpdatedAt = DateTime.UtcNow;
        await _bookingRepo.UpdateAsync(booking);
    }

    public async Task ApproveAsync(int bookingId, int adminId, bool force = false, string? reason = null)
    {
        var booking = await _bookingRepo.GetByIdAsync(bookingId)
            ?? throw new Exception("Booking not found.");

        if (booking.Status != BookingStatus.Pending)
            throw new Exception("Only pending bookings can be approved.");

        var conflicts = (await _bookingRepo.GetApprovedBookingsForRoom(booking.RoomId))
            .Any(b => booking.StartTime < b.EndTime && booking.EndTime > b.StartTime);

        if (conflicts && !force)
            throw new Exception("Time conflict with an already approved booking. Use force=true to override.");

        booking.Status = BookingStatus.Approved;
        booking.ActionBy = adminId;
        booking.ActionAt = DateTime.UtcNow;
        booking.OverrideReason = force ? (reason ?? "Admin override") : reason;
        booking.UpdatedAt = DateTime.UtcNow;
        await _bookingRepo.UpdateAsync(booking);

        // Auto-reject all other pending bookings for the same room/slot
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

    public async Task RejectAsync(int bookingId, int adminId, string? reason = null)
    {
        var booking = await _bookingRepo.GetByIdAsync(bookingId)
            ?? throw new Exception("Booking not found.");

        if (booking.Status != BookingStatus.Pending)
            throw new Exception("Only pending bookings can be rejected.");

        booking.Status = BookingStatus.Rejected;
        booking.ActionBy = adminId;
        booking.ActionAt = DateTime.UtcNow;
        booking.OverrideReason = reason;
        booking.UpdatedAt = DateTime.UtcNow;
        await _bookingRepo.UpdateAsync(booking);
    }

    // ✅ Receives already-UTC values — DateTime.UtcNow comparison is now correct
    private static void ValidateTimeline(DateTime startUtc, DateTime endUtc)
    {
        if (startUtc >= endUtc) throw new Exception("End time must be after start time.");
        if (startUtc < DateTime.UtcNow) throw new Exception("Cannot book in the past.");
        var duration = endUtc - startUtc;
        if (duration.TotalMinutes < 15) throw new Exception("Minimum booking duration is 15 minutes.");
        if (duration.TotalHours > 8) throw new Exception("Maximum booking duration is 8 hours.");
    }
}