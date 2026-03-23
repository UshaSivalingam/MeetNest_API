using MeetNest.Domain.Enums;

namespace MeetNest.Application.DTOs.Notification;

public class NotificationResponseDto
{
    public int Id { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public int? BookingId { get; set; }
    public int? RoomId { get; set; }
    public string? RoomName { get; set; }
    public bool IsRead { get; set; }
    public DateTime? ScheduledFor { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateReminderDto
{
    // Admin manually requests a reminder for a specific booking end time
    public int BookingId { get; set; }
}

public class UnreadCountDto
{
    public int Count { get; set; }
}