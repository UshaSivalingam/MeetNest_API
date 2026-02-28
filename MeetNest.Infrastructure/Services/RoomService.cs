using MeetNest.Application.DTOs;
using MeetNest.Application.DTOs.Filters;
using MeetNest.Application.DTOs.Room;
using MeetNest.Application.Interfaces.Repositories;
using MeetNest.Application.Interfaces.Services;
using MeetNest.Domain.Entities;

namespace MeetNest.Infrastructure.Services;

public class RoomService : IRoomService
{
    private readonly IRoomRepository _repo;
    public RoomService(IRoomRepository repo) => _repo = repo;

    public async Task<PagedResult<RoomResponseDto>> GetAllAsync(RoomFilterDto filter)
    {
        var paged = await _repo.GetAllAsync(filter);
        return MapPaged(paged);
    }

    public async Task<PagedResult<RoomResponseDto>> GetByBranchAsync(int branchId, RoomFilterDto filter)
    {
        var paged = await _repo.GetByBranchIdAsync(branchId, filter);
        return MapPaged(paged);
    }

    public async Task<List<RoomResponseDto>> GetByBranchIdSimpleAsync(int branchId)
    {
        var rooms = await _repo.GetByBranchIdSimpleAsync(branchId);
        return rooms.Select(MapToDto).ToList();
    }

    public async Task<RoomResponseDto?> GetByIdAsync(int id)
    {
        var room = await _repo.GetByIdWithFacilitiesAsync(id);
        return room is null ? null : MapToDto(room);
    }

    public async Task<int> CreateAsync(CreateRoomDto dto)
    {
        var room = new Room { Name = dto.Name, Capacity = dto.Capacity, BranchId = dto.BranchId };
        await _repo.AddAsync(room);
        return room.Id;
    }

    public async Task UpdateAsync(int id, UpdateRoomDto dto)
    {
        var room = await _repo.GetByIdAsync(id) ?? throw new Exception("Room not found.");
        room.Name = dto.Name;
        room.Capacity = dto.Capacity;
        room.UpdatedAt = DateTime.UtcNow;
        await _repo.UpdateAsync(room);
    }

    public async Task DeleteAsync(int id)
    {
        var room = await _repo.GetByIdAsync(id) ?? throw new Exception("Room not found.");
        await _repo.DeleteAsync(room);
    }

    private static PagedResult<RoomResponseDto> MapPaged(PagedResult<Room> paged) => new()
    {
        Items = paged.Items.Select(MapToDto).ToList(),
        TotalCount = paged.TotalCount,
        Page = paged.Page,
        PageSize = paged.PageSize
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
        Facilities = r.RoomFacilities?
            .Where(rf => rf.Facility.IsActive)
            .Select(rf => new RoomFacilityDto
            {
                Id = rf.Facility.Id,
                Name = rf.Facility.Name,
                Description = rf.Facility.Description
            }).ToList() ?? new()
    };
}
