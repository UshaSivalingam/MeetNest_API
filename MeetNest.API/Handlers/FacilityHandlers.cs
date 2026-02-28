using MeetNest.Application.DTOs.Facility;
using MeetNest.Application.DTOs.Filters;
using MeetNest.Application.Interfaces.Services;

namespace MeetNest.API.Handlers;

public static class FacilityHandlers
{
    // GET /api/facilities?search=&page=1&pageSize=10
    public static async Task<IResult> GetAll(
        IFacilityService service,
        string? search = null,
        int page = 1,
        int pageSize = 10)
    {
        var filter = new FacilityFilterDto { Search = search, Page = page, PageSize = pageSize };
        return Results.Ok(await service.GetAllAsync(filter));
    }

    // GET /api/facilities/simple — for dropdowns
    public static async Task<IResult> GetAllSimple(IFacilityService service)
        => Results.Ok(await service.GetAllSimpleAsync());

    public static async Task<IResult> GetById(int id, IFacilityService service)
    {
        var result = await service.GetByIdAsync(id);
        return result is null ? Results.NotFound("Facility not found.") : Results.Ok(result);
    }

    public static async Task<IResult> Create(
        MeetNest.Application.DTOs.Facility.CreateFacilityDto dto, IFacilityService service)
    {
        var result = await service.CreateAsync(dto);
        return Results.Created($"/api/facilities/{result.Id}", result);
    }

    public static async Task<IResult> Update(
        int id, MeetNest.Application.DTOs.Facility.UpdateFacilityDto dto, IFacilityService service)
    {
        await service.UpdateAsync(id, dto);
        return Results.NoContent();
    }

    public static async Task<IResult> Delete(int id, IFacilityService service)
    {
        await service.DeleteAsync(id);
        return Results.NoContent();
    }
}
