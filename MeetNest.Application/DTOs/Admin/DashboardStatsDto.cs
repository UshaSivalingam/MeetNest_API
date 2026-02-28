namespace MeetNest.Application.DTOs.Admin;

public class DashboardStatsDto
{
    public int TotalBranches { get; set; }
    public int TotalRooms { get; set; }
    public int TotalEmployees { get; set; }
    public int TotalBookings { get; set; }

    public int PendingBookings { get; set; }
    public int ApprovedBookings { get; set; }
    public int RejectedBookings { get; set; }
    public int CancelledBookings { get; set; }

    public List<RecentBookingDto> RecentBookings { get; set; } = new();
    public List<BranchBookingStatDto> BookingsPerBranch { get; set; } = new();
}

public class RecentBookingDto
{
    public int Id { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public string RoomName { get; set; } = string.Empty;
    public string BranchName { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Priority { get; set; } = string.Empty;
}

public class BranchBookingStatDto
{
    public string BranchName { get; set; } = string.Empty;
    public int TotalBookings { get; set; }
    public int PendingBookings { get; set; }
    public int ApprovedBookings { get; set; }
}