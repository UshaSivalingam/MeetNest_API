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
    private readonly IEmailService _emailService;

    public NotificationService(
        INotificationRepository notifRepo,
        IBookingRepository bookingRepo,
        AppDbContext db,
        IEmailService emailService)
    {
        _notifRepo = notifRepo;
        _bookingRepo = bookingRepo;
        _db = db;
        _emailService = emailService;
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

    // ── MARK one as read ──────────────────────────────────────────
    public async Task MarkReadAsync(int notificationId, int userId)
        => await _notifRepo.MarkReadAsync(notificationId);

    // ── MARK all as read ──────────────────────────────────────────
    public async Task MarkAllReadAsync(int userId)
        => await _notifRepo.MarkAllReadAsync(userId);

    // ── Admin manually sets a reminder for booking end ────────────
    // Called via POST /api/notifications/reminder
    // Bell notification is saved with ScheduledFor = booking.EndTime
    // Email is NOT sent here — it fires from NotificationReminderBackgroundService
    // when the ScheduledFor time arrives
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
            ScheduledFor = booking.EndTime,   // bell appears at meeting end
            CreatedAt = DateTime.UtcNow,
        };

        await _notifRepo.AddAsync(notification);
        // Email fires from background service when ScheduledFor <= now
    }

    // ── Internal: called by BookingService.CancelAsync ────────────
    // Bell + email both fire immediately when employee cancels
    public async Task NotifyBookingCancelledAsync(
        int bookingId, int branchId, string roomName,
        string employeeName, DateTime endTime)
    {
        // Load booking details for branch name
        var booking = await _bookingRepo.GetByIdWithDetailsAsync(bookingId);
        var branchName = booking?.Branch?.Name ?? string.Empty;
        var startTime = booking?.StartTime ?? DateTime.UtcNow;

        // Get all active admins for bell notification
        var admins = await _db.Users
            .Where(u => u.IsActive && u.RoleId == (int)UserRole.Admin)
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

        // Email SuperAdmin immediately — fire and forget
        _ = Task.Run(async () =>
        {
            try
            {
                await _emailService.SendBookingCancelledToAdminAsync(
                    employeeName, roomName, branchName, startTime, endTime);
            }
            catch { /* swallow — notification itself succeeded */ }
        });
    }

    // ── Internal: called by AdminBookingService.ApproveAsync ──────
    // Schedules bell at booking.EndTime
    // Email fires from background service when ScheduledFor <= now
    public async Task ScheduleMeetingEndReminderAsync(
        int bookingId, int adminId, string roomName, DateTime endTime)
    {
        // Avoid duplicate reminders
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
            ScheduledFor = endTime,           // bell appears at meeting end
            CreatedAt = DateTime.UtcNow,
        };
        await _notifRepo.AddAsync(notification);
        // Email fires from background service when ScheduledFor <= now
    }

    // ── Mapping ───────────────────────────────────────────────────
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