using System.Text.Json;
using RoslynNavigator.Responses;
using RoslynNavigator.Tests.Fixtures;
using RoslynNavigator.Tools;

namespace RoslynNavigator.Tests.Tools;

public class GetTypeHierarchyTests(TestSolutionFixture fixture) : IClassFixture<TestSolutionFixture>
{
    [Fact]
    public async Task GetTypeHierarchy_FullChain_ReturnsBaseAndDerived()
    {
        var json = await GetTypeHierarchyTool.ExecuteAsync(fixture.WorkspaceManager, "AuditableProduct", ct: TestContext.Current.CancellationToken);
        var result = JsonSerializer.Deserialize<TypeHierarchyResult>(json)!;

        Assert.Contains(result.BaseTypes, bt => bt.Contains("BaseEntity"));
    }

    [Fact]
    public async Task GetTypeHierarchy_AbstractClass_ReturnsDerivedTypes()
    {
        var json = await GetTypeHierarchyTool.ExecuteAsync(fixture.WorkspaceManager, "BaseEntity", ct: TestContext.Current.CancellationToken);
        var result = JsonSerializer.Deserialize<TypeHierarchyResult>(json)!;

        Assert.Contains(result.DerivedTypes, dt => dt.Contains("AuditableProduct"));
    }

    [Fact]
    public async Task GetTypeHierarchy_Interface_ReturnsInterfaces()
    {
        var json = await GetTypeHierarchyTool.ExecuteAsync(fixture.WorkspaceManager, "InMemoryOrderRepository", ct: TestContext.Current.CancellationToken);
        var result = JsonSerializer.Deserialize<TypeHierarchyResult>(json)!;

        Assert.Contains(result.Interfaces, i => i.Contains("IOrderRepository"));
    }
}
