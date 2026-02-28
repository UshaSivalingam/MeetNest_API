using MeetNest.Application.DTOs.Auth;

namespace MeetNest.Application.Interfaces.Services;

public interface IAuthService
{
    Task RegisterAdminAsync(RegisterAdminDto dto);

    Task RegisterEmployeeAsync(RegisterEmployeeDto dto);

    Task<LoginResponseDto> LoginAsync(LoginDto dto);

    Task<LoginResponseDto> RefreshTokenAsync(RefreshTokenDto dto);
    Task<int> SaveChangesAsync();
}