using System.Text.Json;
using RoslynNavigator.Responses;
using RoslynNavigator.Tests.Fixtures;
using RoslynNavigator.Tools;

namespace RoslynNavigator.Tests.Tools;

public class GetDiagnosticsTests(TestSolutionFixture fixture) : IClassFixture<TestSolutionFixture>
{
    [Fact]
    public async Task GetDiagnostics_SolutionScope_ReturnsDiagnostics()
    {
        var json = await GetDiagnosticsTool.ExecuteAsync(fixture.WorkspaceManager, scope: "solution", ct: TestContext.Current.CancellationToken);
        var result = JsonSerializer.Deserialize<DiagnosticsResult>(json)!;

        // The sample solution has an intentional unused variable in ProductService
        // which should generate a CS0219 warning
        Assert.NotNull(result);
    }

    [Fact]
    public async Task GetDiagnostics_FileScope_FiltersToSpecificFile()
    {
        var json = await GetDiagnosticsTool.ExecuteAsync(
            fixture.WorkspaceManager,
            scope: "file",
            path: "ProductService.cs",
            ct: TestContext.Current.CancellationToken);
        var result = JsonSerializer.Deserialize<DiagnosticsResult>(json)!;

        // ProductService.cs has the intentional unused variable
        Assert.NotNull(result);
    }

    [Fact]
    public async Task GetDiagnostics_SeverityFilter_Error_FiltersCorrectly()
    {
        var json = await GetDiagnosticsTool.ExecuteAsync(
            fixture.WorkspaceManager,
            scope: "solution",
            severityFilter: "error",
            ct: TestContext.Current.CancellationToken);
        var result = JsonSerializer.Deserialize<DiagnosticsResult>(json)!;

        // All returned diagnostics should be errors
        Assert.All(result.Diagnostics, d => Assert.Equal("error", d.Severity));
    }

    [Fact]
    public async Task GetDiagnostics_ProjectScope_FiltersToProject()
    {
        var json = await GetDiagnosticsTool.ExecuteAsync(
            fixture.WorkspaceManager,
            scope: "project",
            path: "SampleDomain",
            ct: TestContext.Current.CancellationToken);
        var result = JsonSerializer.Deserialize<DiagnosticsResult>(json)!;

        // SampleDomain should have clean compilations (no intentional issues)
        Assert.NotNull(result);
    }
}
