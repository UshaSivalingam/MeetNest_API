
using MeetNest.Domain.Entities;

namespace MeetNest.Application.Interfaces.Repositories;

public interface IRoomFacilityRepository
{
    Task AddAsync(RoomFacility entity);
    Task<bool> ExistsAsync(int RoomId, int facilityId);
    Task<List<RoomFacility>> GetByRoomIdAsync(int roomId);
    Task<bool> FacilityHasRoomsAsync(int facilityId);
    Task RemoveAsync(RoomFacility entity);
    Task<RoomFacility?> GetAsync(int roomId, int facilityId);
    Task SaveChangesAsync();
}
