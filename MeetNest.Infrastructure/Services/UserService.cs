using MeetNest.Application.DTOs;
using MeetNest.Application.DTOs.Filters;
using MeetNest.Application.DTOs.User;
using MeetNest.Application.Interfaces.Repositories;
using MeetNest.Application.Interfaces.Services;
using MeetNest.Domain.Entities;
using MeetNest.Infrastructure.Security;

namespace MeetNest.Infrastructure.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _repo;
    private readonly PasswordHasher _hasher;

    public UserService(IUserRepository repo, PasswordHasher hasher)
    {
        _repo = repo;
        _hasher = hasher;
    }

    public async Task<PagedResult<UserResponseDto>> GetAllAsync(UserFilterDto filter)
    {
        var paged = await _repo.GetAllAsync(filter);
        return MapPaged(paged);
    }

    public async Task<PagedResult<UserResponseDto>> GetAllEmployeesAsync(UserFilterDto filter)
    {
        var paged = await _repo.GetAllEmployeesAsync(filter);
        return MapPaged(paged);
    }

    public async Task<UserResponseDto?> GetByIdAsync(int id)
    {
        var u = await _repo.GetByIdAsync(id);
        return u is null ? null : MapToDto(u);
    }

    public async Task UpdateAsync(int id, UpdateUserDto dto)
    {
        var u = await _repo.GetByIdAsync(id) ?? throw new Exception("User not found.");
        u.FullName = dto.FullName;
        u.BranchId = dto.BranchId;
        u.UpdatedAt = DateTime.UtcNow;
        await _repo.UpdateAsync(u);
    }

    public async Task DeactivateAsync(int id)
    {
        var u = await _repo.GetByIdAsync(id) ?? throw new Exception("User not found.");
        u.IsActive = false;
        u.UpdatedAt = DateTime.UtcNow;
        await _repo.UpdateAsync(u);
    }

    public async Task ResetPasswordAsync(int id, ResetPasswordDto dto)
    {
        var u = await _repo.GetByIdAsync(id) ?? throw new Exception("User not found.");
        u.PasswordHash = _hasher.Hash(dto.NewPassword);
        u.MustChangePassword = true;
        u.UpdatedAt = DateTime.UtcNow;
        await _repo.UpdateAsync(u);
    }

    private static PagedResult<UserResponseDto> MapPaged(PagedResult<User> paged) => new()
    {
        Items = paged.Items.Select(MapToDto).ToList(),
        TotalCount = paged.TotalCount,
        Page = paged.Page,
        PageSize = paged.PageSize
    };

    private static UserResponseDto MapToDto(User u) => new()
    {
        Id = u.Id,
        FullName = u.FullName,
        Email = u.Email,
        Role = u.Role?.Name.ToString() ?? string.Empty,
        BranchId = u.BranchId,
        BranchName = u.Branch?.Name,
        IsActive = u.IsActive,
        MustChangePassword = u.MustChangePassword,
        CreatedAt = u.CreatedAt
    };
}
