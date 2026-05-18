using CommonLogging;
using PortalContracts;
using PortalEngine;

var builder = WebApplication.CreateBuilder(args);

// Configure for Windows Service
if (args.Contains("--windows-service"))
{
    builder.Host.UseWindowsService();
}

builder.Logging.ClearProviders();
builder.Logging.AddPortalLogging();

builder.Services.AddScoped<IPortalEngine, PortalEngineService>();
builder.Services.AddControllers();
builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapControllers();

app.Run();
