using MeetNest.Domain.Enums;

namespace MeetNest.Application.DTOs.Booking;

public class BookingResponseDto
{
    public int Id { get; set; }
    public string RoomName { get; set; } = string.Empty;
    public string BranchName { get; set; } = string.Empty;
    public string EmployeeName { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Priority { get; set; } = string.Empty;
    public string? OverrideReason { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ActionAt { get; set; }
    public List<string> Facilities { get; set; } = new();
}