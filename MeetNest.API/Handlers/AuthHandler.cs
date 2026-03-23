using MeetNest.Application.DTOs.Auth;
using MeetNest.Application.Interfaces.Services;

namespace MeetNest.API.Handlers;

public static class AuthHandlers
{
    public static async Task<IResult> RegisterAdmin(
        RegisterAdminDto dto,
        IAuthService service)
    {
        try
        {
            await service.RegisterAdminAsync(dto);
            return Results.Ok(new { message = "Admin registered successfully." });
        }
        catch (Exception ex)
        {
            return Results.BadRequest(new { error = ex.Message });
        }
    }

    public static async Task<IResult> RegisterEmployee(
        RegisterEmployeeDto dto,
        IAuthService service)
    {
        try
        {
            await service.RegisterEmployeeAsync(dto);
            return Results.Ok(new { message = "Employee registered successfully." });
        }
        catch (Exception ex)
        {
            return Results.BadRequest(new { error = ex.Message });
        }
    }

    public static async Task<IResult> Login(
        LoginDto dto,
        IAuthService service)
    {
        try
        {
            var result = await service.LoginAsync(dto);
            return Results.Ok(result);
        }
        catch (Exception ex)
        {
            return Results.BadRequest(new { error = ex.Message });
        }
    }
}