// MeetNest.Infrastructure/Services/RoomService.cs
// ── REPLACE your existing file entirely ──

using MeetNest.Application.DTOs;
using MeetNest.Application.DTOs.Room;
using MeetNest.Application.DTOs.Filters;
using MeetNest.Application.Interfaces.Repositories;
using MeetNest.Application.Interfaces.Services;
using MeetNest.Domain.Entities;

namespace MeetNest.Infrastructure.Services;

public class RoomService(IRoomRepository repo) : IRoomService
{
    // ── Queries ───────────────────────────────────────────────────
    public async Task<PagedResult<RoomResponseDto>> GetAllAsync(RoomFilterDto filter)
        => MapPaged(await repo.GetAllAsync(filter));

    public async Task<PagedResult<RoomResponseDto>> GetByBranchAsync(int branchId, RoomFilterDto filter)
        => MapPaged(await repo.GetByBranchIdAsync(branchId, filter));

    public async Task<List<RoomResponseDto>> GetByBranchIdSimpleAsync(int branchId)
    {
        var rooms = await repo.GetByBranchIdSimpleAsync(branchId);
        return rooms.Select(MapToDto).ToList();
    }

    public async Task<RoomResponseDto?> GetByIdAsync(int id)
    {
        var room = await repo.GetByIdWithFacilitiesAsync(id);
        return room is null ? null : MapToDto(room);
    }

    // ── Create — now persists ApprovalRequired ────────────────────
    public async Task<int> CreateAsync(CreateRoomDto dto)
    {
        var room = new Room
        {
            Name = dto.Name.Trim(),
            Capacity = dto.Capacity,
            BranchId = dto.BranchId,
            ApprovalRequired = dto.ApprovalRequired,   // ← NEW
        };
        await repo.AddAsync(room);
        return room.Id;
    }

    // ── Update — now persists ApprovalRequired ────────────────────
    public async Task UpdateAsync(int id, UpdateRoomDto dto)
    {
        var room = await repo.GetByIdAsync(id)
            ?? throw new Exception("Room not found.");

        room.Name = dto.Name.Trim();
        room.Capacity = dto.Capacity;
        room.ApprovalRequired = dto.ApprovalRequired;  // ← NEW
        room.UpdatedAt = DateTime.UtcNow;

        await repo.UpdateAsync(room);
    }

    // ── Delete ────────────────────────────────────────────────────
    public async Task DeleteAsync(int id)
    {
        var room = await repo.GetByIdAsync(id)
            ?? throw new Exception("Room not found.");
        await repo.DeleteAsync(room);
    }

    // ── Mapping helpers ───────────────────────────────────────────
    private static PagedResult<RoomResponseDto> MapPaged(PagedResult<Room> paged) => new()
    {
        Items = paged.Items.Select(MapToDto).ToList(),
        TotalCount = paged.TotalCount,
        Page = paged.Page,
        PageSize = paged.PageSize,
    };

    private static RoomResponseDto MapToDto(Room r) => new()
    {
        Id = r.Id,
        Name = r.Name,
        Capacity = r.Capacity,
        BranchId = r.BranchId,
        BranchName = r.Branch?.Name ?? string.Empty,
        IsActive = r.IsActive,
        CreatedAt = r.CreatedAt,
        ApprovalRequired = r.ApprovalRequired,          // ← NEW
        Facilities = r.RoomFacilities?
            .Where(rf => rf.Facility.IsActive)
            .Select(rf => new RoomFacilityDto
            {
                Id = rf.Facility.Id,
                Name = rf.Facility.Name,
                Description = rf.Facility.Description,
            })
            .ToList() ?? new(),
    };
}