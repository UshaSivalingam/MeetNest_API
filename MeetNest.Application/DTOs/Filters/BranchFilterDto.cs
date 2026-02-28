namespace MeetNest.Application.DTOs.Filters;

/// <summary>Filters for GET /api/branches</summary>
public class BranchFilterDto
{
    public string? Search { get; set; }   // matches Name, City, Area, Country
    public string? City { get; set; }
    public string? Country { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}