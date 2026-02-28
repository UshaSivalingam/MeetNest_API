namespace MeetNest.Application.DTOs.Admin;

public class AdminBookingResponseDto
{
    public int Id { get; set; }
    public int RoomId { get; set; }
    public string RoomName { get; set; } = string.Empty;
    public int UserId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public string EmployeeEmail { get; set; } = string.Empty;
    public int BranchId { get; set; }
    public string BranchName { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Priority { get; set; } = string.Empty;
    public string? OverrideReason { get; set; }
    public int? ActionBy { get; set; }
    public DateTime? ActionAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<string> Facilities { get; set; } = new();
}

public class AdminBookingFilterDto
{
    public string? Status { get; set; }         // "Pending", "Approved", etc.
    public int? BranchId { get; set; }
    public DateTime? From { get; set; }
    public DateTime? To { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}

public class PagedAdminBookingsDto
{
    public List<AdminBookingResponseDto> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
}

public class BookingActionBodyDto
{
    public string? Reason { get; set; }
    public bool Force { get; set; } = false;
}