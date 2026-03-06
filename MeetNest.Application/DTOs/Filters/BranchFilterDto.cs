namespace MeetNest.Application.DTOs.Filters;

public class BranchFilterDto
{
    public string? Search { get; set; }
    public string? City { get; set; }
    public string? Country { get; set; }
    public string? SortBy { get; set; }   // "name" | "city" | "country" | "rooms"
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}