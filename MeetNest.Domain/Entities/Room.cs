using MeetNest.Domain.Entities;

public class Room
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public int Capacity { get; set; }

    public int BranchId { get; set; }
    public Branch Branch { get; set; } = null!;

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    public ICollection<RoomFacility> RoomFacilities { get; set; } = new List<RoomFacility>();

    public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
}