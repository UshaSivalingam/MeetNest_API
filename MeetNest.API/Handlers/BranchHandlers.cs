using MeetNest.Application.DTOs.Filters;
using MeetNest.Application.Interfaces.Services;
using MeetNest.Domain.Entities;

namespace MeetNest.API.Handlers;

public static class BranchHandlers
{
    // GET /api/branches?search=&city=&country=&sortBy=name&page=1&pageSize=6
    public static async Task<IResult> GetAll(
        IBranchService service,
        string? search = null,
        string? city = null,
        string? country = null,
        string? sortBy = "name",
        int page = 1,
        int pageSize = 6)
    {
        var filter = new BranchFilterDto
        {
            Search = search,
            City = city,
            Country = country,
            SortBy = sortBy,
            Page = page,
            PageSize = pageSize,
        };
        return Results.Ok(await service.GetAllAsync(filter));
    }

    // GET /api/branches/simple  — for dropdowns (no pagination)
    public static async Task<IResult> GetAllSimple(IBranchService service)
        => Results.Ok(await service.GetAllSimpleAsync());

    public static async Task<IResult> GetById(int id, IBranchService service)
        => Results.Ok(await service.GetByIdAsync(id));

    public static async Task<IResult> Create(Branch branch, IBranchService service)
    {
        await service.CreateAsync(branch);
        return Results.Ok("Branch created successfully.");
    }

    public static async Task<IResult> Update(int id, Branch branch, IBranchService service)
    {
        await service.UpdateAsync(id, branch);
        return Results.Ok("Branch updated successfully.");
    }

    public static async Task<IResult> Delete(int id, IBranchService service)
    {
        await service.DeleteAsync(id);
        return Results.Ok("Branch deleted.");
    }
}