namespace MeetNest.Application.DTOs.Auth;

public class RegisterEmployeeDto
{
    public string FullName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;

    public int? BranchId { get; set; }
}