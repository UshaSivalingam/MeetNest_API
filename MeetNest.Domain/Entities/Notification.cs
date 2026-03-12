using MeetNest.Domain.Enums;

namespace MeetNest.Domain.Entities;

public class Notification
{
    public int Id { get; set; }

    // Who receives this notification (always an Admin user)
    public int UserId { get; set; }
    public User User { get; set; } = null!;

    public NotificationType Type { get; set; }

    public string Message { get; set; } = string.Empty;

    // Optional link back to the booking that triggered this
    public int? BookingId { get; set; }
    public Booking? Booking { get; set; }

    // Optional link to room (for room-status reminders)
    public int? RoomId { get; set; }
    public Room? Room { get; set; }

    public bool IsRead { get; set; } = false;

    // null  → show immediately
    // future → show only after this time (e.g. meeting end reminder)
    public DateTime? ScheduledFor { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}