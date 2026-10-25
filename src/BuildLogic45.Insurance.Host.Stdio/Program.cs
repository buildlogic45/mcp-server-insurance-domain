using BuildLogic45.Insurance.Core;
using BuildLogic45.Insurance.Infrastructure;
using BuildLogic45.Insurance.Mcp;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using Serilog;

var builder = Host.CreateApplicationBuilder(args);

// stdout carries the MCP protocol, so console logs must go to stderr.
builder.Logging.AddConsole(options => options.LogToStandardErrorThreshold = LogLevel.Trace);

// One line per entry, no ANSI colors, so logs read cleanly in the client's MCP output panel.
builder.Logging.AddSimpleConsole(options =>
{
    options.SingleLine = true;
    options.TimestampFormat = "HH:mm:ss ";
    options.ColorBehavior = LoggerColorBehavior.Disabled;
});

// Some clients (e.g. the Copilot agent runtime) discard server stderr, so also log to a file.
// Shared mode lets several server instances append to the same daily file.
var logFilePath = builder.Configuration["Logging:File:Path"]
    ?? Path.Combine(AppContext.BaseDirectory, "logs", "insurance-stdio-.log");
builder.Logging.AddSerilog(
    new LoggerConfiguration()
        .MinimumLevel.Information()
        .WriteTo.File(
            logFilePath,
            rollingInterval: RollingInterval.Day,
            shared: true,
            outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level:u3}] [pid {ProcessId}] {SourceContext}: {Message:lj}{NewLine}{Exception}")
        .Enrich.WithProperty("ProcessId", Environment.ProcessId)
        .CreateLogger(),
    dispose: true);

builder.Services
    .AddInsuranceCore()
    .AddInsuranceInfrastructure(builder.Configuration);

builder.Services
    .AddMcpServer()
    .WithStdioServerTransport()
    .WithInsuranceCapabilities();

await builder.Build().RunAsync();
