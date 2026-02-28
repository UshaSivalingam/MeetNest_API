using MeetNest.Application.DTOs.Auth;
using MeetNest.Application.Interfaces.Services;

namespace MeetNest.API.Handlers;

public static class AuthHandlers
{
    public static async Task<IResult> RegisterAdmin(
        RegisterAdminDto dto,
        IAuthService service)
    {
        await service.RegisterAdminAsync(dto);
        return Results.Ok("Admin registered successfully.");
    }

    public static async Task<IResult> RegisterEmployee(
        RegisterEmployeeDto dto,
        IAuthService service)
    {
        await service.RegisterEmployeeAsync(dto);
        return Results.Ok("Employee registered successfully.");
    }

    public static async Task<IResult> Login(
        LoginDto dto,
        IAuthService service)
    {
        var result = await service.LoginAsync(dto);
        return Results.Ok(result);
    }
}