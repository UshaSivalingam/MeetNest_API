using MeetNest.Application.DTOs.RoomFacility;

namespace MeetNest.Application.Interfaces.Services;

public interface IRoomFacilityService
{
    Task AssignFacilityAsync(CreateRoomFacilityDto dto);
    Task<List<RoomFacilityItemDto>> GetFacilitiesByRoomAsync(int roomId);
    Task<bool> RemoveFacilityAsync(int roomId, int facilityId);
}