// MeetNest.Infrastructure/Services/AuthService.cs

using MeetNest.Application.DTOs.Auth;
using MeetNest.Application.Interfaces.Repositories;
using MeetNest.Application.Interfaces.Security;
using MeetNest.Application.Interfaces.Services;
using MeetNest.Domain.Entities;
using MeetNest.Domain.Enums;
using MeetNest.Infrastructure.Security;
using Microsoft.Extensions.Configuration;

namespace MeetNest.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepo;
    private readonly IBranchRepository _branchRepo;
    private readonly IJwtTokenGenerator _jwt;
    private readonly PasswordHasher _hasher;
    private readonly IConfiguration _config;

    public AuthService(
        IUserRepository userRepo,
        IBranchRepository branchRepo,
        IJwtTokenGenerator jwt,
        PasswordHasher hasher,
        IConfiguration config)
    {
        _userRepo = userRepo;
        _branchRepo = branchRepo;
        _jwt = jwt;
        _hasher = hasher;
        _config = config;
    }

    // 🔐 Admin Registration (Only One)
    public async Task RegisterAdminAsync(RegisterAdminDto dto)
    {
        if (await _userRepo.AdminExistsAsync())
            throw new Exception("Admin already exists.");

        var user = new User
        {
            FullName = dto.FullName,
            Email = dto.Email,
            PasswordHash = _hasher.Hash(dto.Password),
            RoleId = (int)UserRole.Admin,
            CreatedAt = DateTime.UtcNow
        };

        await _userRepo.AddAsync(user);
        await _userRepo.SaveChangesAsync();
    }

    // 👩‍💻 Employee Registration
    public async Task RegisterEmployeeAsync(RegisterEmployeeDto dto)
    {
        // ── Company domain validation ─────────────────────────────
        var companyDomain = _config["CompanySettings:Domain"];
        var emailDomain = dto.Email.Split('@')[1];
        if (!emailDomain.Equals(companyDomain, StringComparison.OrdinalIgnoreCase))
            throw new Exception("Invalid company email domain.");

        // ── Branch validation ─────────────────────────────────────
        if (!dto.BranchId.HasValue)
            throw new Exception("Branch is required.");

        var branchExists = await _branchRepo.ExistsAsync(dto.BranchId.Value);
        if (!branchExists)
            throw new Exception("Invalid branch.");

        // ── Duplicate email check ─────────────────────────────────
        var existing = await _userRepo.GetByEmailAsync(dto.Email);
        if (existing is not null)
            throw new Exception("An account with this email already exists.");

        // ── Create user ───────────────────────────────────────────
        var user = new User
        {
            FullName = dto.FullName,
            Email = dto.Email,
            PasswordHash = _hasher.Hash(dto.Password),
            RoleId = (int)UserRole.Employee,
            BranchId = dto.BranchId,
            MustChangePassword = false,
            CreatedAt = DateTime.UtcNow
        };

        await _userRepo.AddAsync(user);
        await _userRepo.SaveChangesAsync();
    }

    // 🔑 Login
    public async Task<LoginResponseDto> LoginAsync(LoginDto dto)
    {
        var user = await _userRepo.GetByEmailAsync(dto.Email)
            ?? throw new Exception("Invalid credentials.");

        if (!_hasher.Verify(dto.Password, user.PasswordHash))
            throw new Exception("Invalid credentials.");

        var accessToken = _jwt.GenerateAccessToken(user);
        var refreshToken = _jwt.GenerateRefreshToken();

        return new LoginResponseDto
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            ExpiresAt = DateTime.UtcNow.AddMinutes(30),
            User = new UserDto
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                Role = user.Role.Name.ToString()
            }
        };
    }

    // 🔄 Refresh Token
    public Task<LoginResponseDto> RefreshTokenAsync(RefreshTokenDto dto)
    {
        throw new NotImplementedException();
    }

    public async Task<int> SaveChangesAsync()
    {
        return await _userRepo.SaveChangesAsync();
    }
}