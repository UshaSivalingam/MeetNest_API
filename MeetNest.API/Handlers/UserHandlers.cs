using MeetNest.Application.DTOs.Filters;
using MeetNest.Application.DTOs.User;
using MeetNest.Application.Interfaces.Services;

namespace MeetNest.API.Handlers;

public static class UserHandlers
{
    // GET /api/admin/users?search=&branchId=&role=&isActive=&page=1&pageSize=10
    public static async Task<IResult> GetAll(
        IUserService service,
        string? search = null,
        int? branchId = null,
        string? role = null,
        bool? isActive = null,
        int page = 1,
        int pageSize = 10)
    {
        var filter = new UserFilterDto
        {
            Search = search,
            BranchId = branchId,
            Role = role,
            IsActive = isActive,
            Page = page,
            PageSize = pageSize
        };
        return Results.Ok(await service.GetAllAsync(filter));
    }

    // GET /api/admin/users/employees?search=&branchId=&page=1&pageSize=10
    public static async Task<IResult> GetAllEmployees(
        IUserService service,
        string? search = null,
        int? branchId = null,
        int page = 1,
        int pageSize = 10)
    {
        var filter = new UserFilterDto
        {
            Search = search,
            BranchId = branchId,
            Page = page,
            PageSize = pageSize
        };
        return Results.Ok(await service.GetAllEmployeesAsync(filter));
    }

    public static async Task<IResult> GetById(int id, IUserService service)
    {
        var user = await service.GetByIdAsync(id);
        return user is null ? Results.NotFound("User not found.") : Results.Ok(user);
    }

    public static async Task<IResult> Update(
        int id,
        MeetNest.Application.DTOs.User.UpdateUserDto dto,
        IUserService service)
    {
        await service.UpdateAsync(id, dto);
        return Results.Ok(new { Message = "User updated." });
    }

    public static async Task<IResult> Deactivate(int id, IUserService service)
    {
        await service.DeactivateAsync(id);
        return Results.Ok(new { Message = "User deactivated." });
    }

    public static async Task<IResult> ResetPassword(
        int id,
        MeetNest.Application.DTOs.User.ResetPasswordDto dto,
        IUserService service)
    {
        await service.ResetPasswordAsync(id, dto);
        return Results.Ok(new { Message = "Password reset. User must change it on next login." });
    }
}
