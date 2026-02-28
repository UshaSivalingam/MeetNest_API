using MeetNest.Application.DTOs;
using MeetNest.Application.DTOs.Facility;
using MeetNest.Application.DTOs.Filters;

namespace MeetNest.Application.Interfaces.Services;

public interface IFacilityService
{
    Task<PagedResult<FacilityResponseDto>> GetAllAsync(FacilityFilterDto filter);
    Task<List<FacilityResponseDto>> GetAllSimpleAsync();   // for dropdowns
    Task<FacilityResponseDto?> GetByIdAsync(int id);
    Task<FacilityResponseDto> CreateAsync(CreateFacilityDto dto);
    Task UpdateAsync(int id, UpdateFacilityDto dto);
    Task DeleteAsync(int id);
}
