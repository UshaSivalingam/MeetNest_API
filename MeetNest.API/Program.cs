using MeetNest.API;

var builder = WebApplication.CreateBuilder(args);
// Add Services
builder.Services.AddApplicationServices(builder.Configuration);
var app = builder.Build();

// Pipeline
app.UseApplicationPipeline();
app.Run();
