namespace MeetNest.Application.DTOs.Filters;

public class UserFilterDto
{
    public string? Search { get; set; }   // matches FullName, Email
    public int? BranchId { get; set; }
    public string? Role { get; set; }   // "Admin" | "Employee"
    public bool? IsActive { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}