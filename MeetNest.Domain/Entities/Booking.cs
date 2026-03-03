using MeetNest.Domain.Enums;

namespace MeetNest.Domain.Entities;

public class Booking
{
    public int Id { get; set; }

    public int RoomId { get; set; }
    public Room Room { get; set; } = null!;

    public int UserId { get; set; }       // FK to User
    public User User { get; set; } = null!;  // Navigation property

    public int BranchId { get; set; }
    public Branch Branch { get; set; } = null!;

    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }

    public BookingPriority Priority { get; set; } = BookingPriority.Low;

    public BookingStatus Status { get; set; } = BookingStatus.Pending;

    public string? Notes { get; set; }
    // Audit Fields
    public int? ActionBy { get; set; }  // Admin Id
    public DateTime? ActionAt { get; set; }
    public string? OverrideReason { get; set; }

    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}