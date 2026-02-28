using MeetNest.Domain.Entities;

public class RefreshToken
{
    public int Id { get; set; }

    public string Token { get; set; } = string.Empty;

    public int UserId { get; set; }

    public User User { get; set; } = null!;

    public DateTime Expires { get; set; }

    public bool IsRevoked { get; set; } = false;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}