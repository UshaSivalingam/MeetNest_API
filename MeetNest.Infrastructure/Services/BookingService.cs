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
        ValidateTimeline(dto.StartTime, dto.EndTime);

        var room = await _roomRepo.GetByIdAsync(dto.RoomId) ?? throw new Exception("Room not found.");
        var user = await _userRepo.GetByIdAsync(userId) ?? throw new Exception("User not found.");

        if (room.BranchId != user.BranchId)
            throw new Exception("Cannot book room from another branch.");

        var startUtc = DateTime.SpecifyKind(dto.StartTime, DateTimeKind.Utc);
        var endUtc = DateTime.SpecifyKind(dto.EndTime, DateTimeKind.Utc);

        if (await _bookingRepo.IsRoomBookedAsync(dto.RoomId, startUtc, endUtc))
            throw new Exception("Room is already booked for the selected time slot.");

        var booking = new Booking
        {
            RoomId = dto.RoomId,
            UserId = userId,
            BranchId = room.BranchId,
            StartTime = startUtc,
            EndTime = endUtc,
            Status = BookingStatus.Pending
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
                EmployeeName = b.User.FullName,
                StartTime = b.StartTime,
                EndTime = b.EndTime,
                Status = b.Status.ToString(),
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

    public async Task CancelAsync(int bookingId, int userId)
    {
        var booking = await _bookingRepo.GetByIdAsync(bookingId) ?? throw new Exception("Booking not found.");
        if (booking.UserId != userId) throw new Exception("Unauthorized.");
        if (booking.Status == BookingStatus.Approved && booking.StartTime <= DateTime.UtcNow)
            throw new Exception("Cannot cancel an ongoing booking.");
        booking.Status = BookingStatus.Cancelled;
        await _bookingRepo.UpdateAsync(booking);
    }

    public async Task ApproveAsync(int bookingId, int adminId, bool force = false, string? reason = null)
    {
        var booking = await _bookingRepo.GetByIdAsync(bookingId) ?? throw new Exception("Booking not found.");
        if (booking.Status != BookingStatus.Pending) throw new Exception("Only pending bookings can be approved.");

        var conflicts = (await _bookingRepo.GetApprovedBookingsForRoom(booking.RoomId))
            .Any(b => booking.StartTime < b.EndTime && booking.EndTime > b.StartTime);

        if (conflicts && !force) throw new Exception("Time conflict exists.");

        booking.Status = BookingStatus.Approved;
        booking.ActionBy = adminId;
        booking.ActionAt = DateTime.UtcNow;
        booking.OverrideReason = force ? (reason ?? "Admin override") : reason;
        await _bookingRepo.UpdateAsync(booking);
    }

    public async Task RejectAsync(int bookingId, int adminId)
    {
        var booking = await _bookingRepo.GetByIdAsync(bookingId) ?? throw new Exception("Booking not found.");
        booking.Status = BookingStatus.Rejected;
        booking.ActionBy = adminId;
        booking.ActionAt = DateTime.UtcNow;
        await _bookingRepo.UpdateAsync(booking);
    }

    private static void ValidateTimeline(DateTime start, DateTime end)
    {
        if (start >= end) throw new Exception("Invalid time range.");
        if (start < DateTime.UtcNow) throw new Exception("Cannot book in the past.");
        var duration = end - start;
        if (duration.TotalMinutes < 15) throw new Exception("Minimum duration is 15 minutes.");
        if (duration.TotalHours > 8) throw new Exception("Maximum duration is 8 hours.");
    }
}
