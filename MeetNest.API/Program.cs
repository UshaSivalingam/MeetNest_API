using MeetNest.API;
using MeetNest.API.Endpoints;
using MeetNest.API.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Add Services
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddApplicationServices(builder.Configuration);

var app = builder.Build();

// Middleware Pipeline
app.UseSwagger();
app.UseSwaggerUI();

app.UseMiddleware<GlobalExceptionMiddleware>();

app.UseCors("AllowReact");
app.UseAuthentication();
app.UseAuthorization();

// ── Employee / Shared Endpoints (existing) ────────────────────────────────────
app.MapAuthEndpoints();
app.MapBranchEndpoints();
app.MapRoomEndpoints();
app.MapFacilityEndpoints();
app.MapRoomFacilityEndpoints();
app.MapBookingEndpoints();

// ── Admin Dashboard Endpoints (NEW) ──────────────────────────────────────────
app.MapAdminDashboardEndpoints();
app.MapAdminBookingEndpoints();
app.MapUserEndpoints();

app.Run();
