// MeetNest.Infrastructure/Services/NotificationService.cs
using MeetNest.Application.DTOs.Notification;
using MeetNest.Application.Interfaces.Repositories;
using MeetNest.Application.Interfaces.Services;
using MeetNest.Domain.Entities;
using MeetNest.Domain.Enums;
using MeetNest.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace MeetNest.Infrastructure.Services;

public class NotificationService : INotificationService
{
    private readonly INotificationRepository _notifRepo;
    private readonly IBookingRepository _bookingRepo;
    private readonly AppDbContext _db;

    public NotificationService(
        INotificationRepository notifRepo,
        IBookingRepository bookingRepo,
        AppDbContext db)
    {
        _notifRepo = notifRepo;
        _bookingRepo = bookingRepo;
        _db = db;
    }

    // ── GET: due + unread notifications for a user ────────────────
    public async Task<List<NotificationResponseDto>> GetDueForUserAsync(int userId)
    {
        var list = await _notifRepo.GetDueForUserAsync(userId);
        return list.Select(MapToDto).ToList();
    }

    // ── GET: unread badge count ───────────────────────────────────
    public async Task<int> GetUnreadCountAsync(int userId)
        => await _notifRepo.GetUnreadCountAsync(userId);

    // ── MARK one as read ─────────────────────────────────────────
    public async Task MarkReadAsync(int notificationId, int userId)
        => await _notifRepo.MarkReadAsync(notificationId);

    // ── MARK all as read ─────────────────────────────────────────
    public async Task MarkAllReadAsync(int userId)
        => await _notifRepo.MarkAllReadAsync(userId);

    // ── Admin manually sets a reminder for booking end ────────────
    // Called via POST /api/notifications/reminder
    public async Task CreateReminderAsync(int bookingId, int adminId)
    {
        var booking = await _bookingRepo.GetByIdWithDetailsAsync(bookingId)
            ?? throw new Exception("Booking not found.");

        var notification = new Notification
        {
            UserId = adminId,
            Type = NotificationType.MeetingEndReminder,
            Message = $"🔔 Reminder: Meeting in {booking.Room.Name} ends at {booking.EndTime:HH:mm} UTC. You can now update the room status.",
            BookingId = bookingId,
            RoomId = booking.RoomId,
            IsRead = false,
            ScheduledFor = booking.EndTime,  // show exactly at meeting end
            CreatedAt = DateTime.UtcNow,
        };

        await _notifRepo.AddAsync(notification);
    }

    // ── Internal: called by BookingService.CancelAsync ───────────
    // Notifies ALL admins immediately when an employee cancels
    public async Task NotifyBookingCancelledAsync(
        int bookingId, int branchId, string roomName, string employeeName, DateTime endTime)
    {
        // Get all active admins (admins are global, not branch-scoped)
        var admins = await _db.Users
            .Include(u => u.Role)
            .Where(u => u.IsActive && u.Role.Name == UserRole.Admin)
            .ToListAsync();

        foreach (var admin in admins)
        {
            var notification = new Notification
            {
                UserId = admin.Id,
                Type = NotificationType.BookingCancelled,
                Message = $"🚫 Booking cancelled: {employeeName} cancelled their booking for {roomName}. The room is now free — you can change its status or delete it.",
                BookingId = bookingId,
                RoomId = null,
                IsRead = false,
                ScheduledFor = null,          // show immediately
                CreatedAt = DateTime.UtcNow,
            };
            await _notifRepo.AddAsync(notification);
        }
    }

    // ── Internal: called by AdminBookingService.ApproveAsync ──────
    // Schedules a reminder at booking.EndTime for the approving admin
    public async Task ScheduleMeetingEndReminderAsync(
        int bookingId, int adminId, string roomName, DateTime endTime)
    {
        // Avoid duplicate reminders: check if one already exists for this booking+admin
        var already = await _db.Notifications.AnyAsync(n =>
            n.BookingId == bookingId &&
            n.UserId == adminId &&
            n.Type == NotificationType.MeetingEndReminder);

        if (already) return;

        var notification = new Notification
        {
            UserId = adminId,
            Type = NotificationType.MeetingEndReminder,
            Message = $"🔔 Meeting completed: {roomName} is now free. You can change its status or delete it.",
            BookingId = bookingId,
            RoomId = null,
            IsRead = false,
            ScheduledFor = endTime,           // show at meeting end time
            CreatedAt = DateTime.UtcNow,
        };
        await _notifRepo.AddAsync(notification);
    }

    // ── Mapping ──────────────────────────────────────────────────
    private static NotificationResponseDto MapToDto(Notification n) => new()
    {
        Id = n.Id,
        Type = n.Type.ToString(),
        Message = n.Message,
        BookingId = n.BookingId,
        RoomId = n.RoomId,
        RoomName = n.Room?.Name,
        IsRead = n.IsRead,
        ScheduledFor = n.ScheduledFor,
        CreatedAt = n.CreatedAt,
    };
}