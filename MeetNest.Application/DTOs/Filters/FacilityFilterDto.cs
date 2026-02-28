namespace MeetNest.Application.DTOs.Filters;

public class FacilityFilterDto
{
    public string? Search { get; set; }   // matches Name, Description
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}