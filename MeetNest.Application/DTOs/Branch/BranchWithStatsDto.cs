namespace MeetNest.Application.DTOs.Branch;

public class BranchWithStatsDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Area { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public int TotalRooms { get; set; }
    public int TotalEmployees { get; set; }
    public int TotalBookings { get; set; }
    public DateTime CreatedAt { get; set; }
}