using MeetNest.Application.DTOs.Notification;

namespace MeetNest.Application.Interfaces.Services;

public interface INotificationService
{
    // Called by GET /api/notifications — returns due, unread notifications
    Task<List<NotificationResponseDto>> GetDueForUserAsync(int userId);

    // Bell badge count
    Task<int> GetUnreadCountAsync(int userId);

    // Mark one as read
    Task MarkReadAsync(int notificationId, int userId);

    // Mark all as read
    Task MarkAllReadAsync(int userId);

    // Admin manually sets a reminder for a booking's end time
    Task CreateReminderAsync(int bookingId, int adminId);

    // Internal: called by BookingService on cancel — notifies all admins in branch
    Task NotifyBookingCancelledAsync(int bookingId, int branchId, string roomName, string employeeName, DateTime endTime);

    // Internal: called by AdminBookingService on approve — schedules end-time reminder
    Task ScheduleMeetingEndReminderAsync(int bookingId, int adminId, string roomName, DateTime endTime);
}