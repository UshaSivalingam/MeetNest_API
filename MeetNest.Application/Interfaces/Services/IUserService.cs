using MeetNest.Application.DTOs;
using MeetNest.Application.DTOs.Filters;
using MeetNest.Application.DTOs.User;

namespace MeetNest.Application.Interfaces.Services;

public interface IUserService
{
    Task<PagedResult<UserResponseDto>> GetAllAsync(UserFilterDto filter);
    Task<PagedResult<UserResponseDto>> GetAllEmployeesAsync(UserFilterDto filter);
    Task<UserResponseDto?> GetByIdAsync(int id);
    Task UpdateAsync(int id, UpdateUserDto dto);
    Task DeactivateAsync(int id);
    Task ResetPasswordAsync(int id, ResetPasswordDto dto);
}
