
namespace MeetNest.Domain.Entities;

public class Room
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Capacity { get; set; }
    public int BranchId { get; set; }

    // true  → Booking stays Pending until Admin approves/rejects
    // false → Booking is auto-approved immediately (if slot is free)
    public bool ApprovalRequired { get; set; } = true;

    // true  → Room is under maintenance, NO new bookings allowed at all
    // false → Room is available for booking
    public bool UnderMaintenance { get; set; } = false;

    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    // Navigation
    public Branch Branch { get; set; } = null!;
    public ICollection<RoomFacility> RoomFacilities { get; set; } = new List<RoomFacility>();
    public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
}