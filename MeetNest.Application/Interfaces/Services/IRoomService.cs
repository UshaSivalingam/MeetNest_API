using MeetNest.Application.DTOs;
using MeetNest.Application.DTOs.Filters;
using MeetNest.Application.DTOs.Room;

namespace MeetNest.Application.Interfaces.Services;

public interface IRoomService
{
    Task<PagedResult<RoomResponseDto>> GetAllAsync(RoomFilterDto filter);
    Task<PagedResult<RoomResponseDto>> GetByBranchAsync(int branchId, RoomFilterDto filter);
    Task<RoomResponseDto?> GetByIdAsync(int id);
    Task<int> CreateAsync(CreateRoomDto dto);
    Task UpdateAsync(int id, UpdateRoomDto dto);
    Task DeleteAsync(int id);

    // Employee — simple list scoped to their branch (no pagination needed for booking picker)
    Task<List<RoomResponseDto>> GetByBranchIdSimpleAsync(int branchId);
}
