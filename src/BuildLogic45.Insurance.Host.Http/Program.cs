using BuildLogic45.Insurance.Core;
using BuildLogic45.Insurance.Infrastructure;
using BuildLogic45.Insurance.Mcp;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddInsuranceCore()
    .AddInsuranceInfrastructure(builder.Configuration);

builder.Services
    .AddMcpServer()
    .WithHttpTransport()
    .WithInsuranceCapabilities();

var app = builder.Build();

app.MapGet("/health", () => Results.Ok(new { status = "healthy" }));
app.MapMcp("/mcp");

app.Run();

// Exposes the entry point to WebApplicationFactory in integration tests.
public partial class Program;
