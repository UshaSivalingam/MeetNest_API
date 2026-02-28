using MeetNest.Domain.Enums;

namespace MeetNest.Domain.Entities;

public class Role
{
    public int Id { get; set; }

    public UserRole Name { get; set; }

    public string Description { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    public ICollection<User> Users { get; set; } = new List<User>();
}