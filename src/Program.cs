using RoslynNavigator;
using Microsoft.Build.Locator;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

// MSBuild locator MUST be called before any Roslyn types are loaded.
// This resolves the MSBuild instance needed by MSBuildWorkspace.
MSBuildLocator.RegisterDefaults();

var builder = Host.CreateApplicationBuilder(args);

// Configure logging
builder.Logging.SetMinimumLevel(LogLevel.Information);
builder.Logging.AddConsole();

// Register workspace services
builder.Services.AddSingleton<WorkspaceManager>();
builder.Services.AddHostedService<WorkspaceInitializer>();

// Configure MCP server with stdio transport
builder.Services.AddMcpServer()
    .WithStdioServerTransport()
    .WithToolsFromAssembly();

// Discover solution path
var solutionPath = SolutionDiscovery.FindSolutionPath(args);
WorkspaceInitializer.SolutionPath = solutionPath;

var app = builder.Build();

var workspaceManager = app.Services.GetRequiredService<WorkspaceManager>();
workspaceManager.Services = app.Services;

var logger = app.Services.GetRequiredService<ILogger<Program>>();
if (solutionPath is not null)
{
    logger.LogInformation("Discovered solution: {SolutionPath}", solutionPath);
}
else
{
    logger.LogWarning(
        "No .sln/.slnx file found in '{SearchDirectory}' (searched {MaxDepth} levels deep). " +
        "Tools will return not-ready status. Use --solution <path> to specify explicitly.",
        Directory.GetCurrentDirectory(),
        SolutionDiscovery.MaxSearchDepth);
}

await app.RunAsync();
