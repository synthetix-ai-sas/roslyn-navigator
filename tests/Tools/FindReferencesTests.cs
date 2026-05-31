using System.Text.Json;
using RoslynNavigator.Responses;
using RoslynNavigator.Tests.Fixtures;
using RoslynNavigator.Tools;

namespace RoslynNavigator.Tests.Tools;

public class FindReferencesTests(TestSolutionFixture fixture) : IClassFixture<TestSolutionFixture>
{
    [Fact]
    public async Task FindReferences_CrossProjectInterface_ReturnsMultipleReferences()
    {
        var json = await FindReferencesTool.ExecuteAsync(fixture.WorkspaceManager, "IOrderRepository", ct: TestContext.Current.CancellationToken);
        var result = JsonSerializer.Deserialize<ReferencesResult>(json)!;

        // IOrderRepository is referenced in Infrastructure (implementations) and Api (OrderService)
        Assert.True(result.Count > 0, "Expected references to IOrderRepository across projects");
    }

    [Fact]
    public async Task FindReferences_ClassUsedInSameFile_ReturnsReferences()
    {
        var json = await FindReferencesTool.ExecuteAsync(fixture.WorkspaceManager, "Order", ct: TestContext.Current.CancellationToken);
        var result = JsonSerializer.Deserialize<ReferencesResult>(json)!;

        // Order is referenced in many places
        Assert.True(result.Count > 0);
    }

    [Fact]
    public async Task FindReferences_NonexistentSymbol_ReturnsZero()
    {
        var json = await FindReferencesTool.ExecuteAsync(fixture.WorkspaceManager, "ZZZNonExistentXXX", ct: TestContext.Current.CancellationToken);
        var result = JsonSerializer.Deserialize<ReferencesResult>(json)!;

        Assert.Equal(0, result.Count);
    }
}
