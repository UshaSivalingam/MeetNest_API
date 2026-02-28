using MeetNest.Domain.Enums;

namespace MeetNest.Domain.Entities;

public class User
{
    public int Id { get; set; }  // int ID for User

    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public bool MustChangePassword { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public int RoleId { get; set; }   // FK to Role
    public Role Role { get; set; } = null!;

    public int? BranchId { get; set; }   // FK to Branch as int
    public Branch? Branch { get; set; }

    public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
}