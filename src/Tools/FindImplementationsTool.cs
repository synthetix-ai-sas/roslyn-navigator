using System.ComponentModel;
using System.Text.Json;
using RoslynNavigator.Responses;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.FindSymbols;
using ModelContextProtocol.Server;

namespace RoslynNavigator.Tools;

[McpServerToolType]
public static class FindImplementationsTool
{
    [McpServerTool(Name = "find_implementations"), Description("Find all types that implement an interface or derive from a base class.")]
    public static async Task<string> ExecuteAsync(
        WorkspaceManager workspace,
        [Description("The interface or base class name to find implementations for")] string interfaceName,
        CancellationToken ct = default)
    {
        var notReady = await workspace.EnsureReadyOrStatusAsync(ct);
        if (notReady is not null) return notReady;

        var solution = workspace.GetSolution();
        if (solution is null)
            return JsonSerializer.Serialize(new ImplementationsResult([]));

        var symbol = await SymbolResolver.ResolveSymbolAsync(workspace, interfaceName, ct: ct);
        if (symbol is not INamedTypeSymbol typeSymbol)
            return JsonSerializer.Serialize(new ImplementationsResult([]));

        var results = new List<ImplementationInfo>();

        if (typeSymbol.TypeKind == TypeKind.Interface)
        {
            var implementations = await SymbolFinder.FindImplementationsAsync(typeSymbol, solution, cancellationToken: ct);
            foreach (var impl in implementations)
            {
                var location = SymbolResolver.GetLocation(impl);
                if (location.HasValue)
                {
                    results.Add(new ImplementationInfo(impl.Name, location.Value.File, location.Value.Line));
                }
            }
        }
        else
        {
            // Find derived classes for non-interface types
            var derived = await SymbolFinder.FindDerivedClassesAsync(typeSymbol, solution, cancellationToken: ct);
            foreach (var d in derived)
            {
                var location = SymbolResolver.GetLocation(d);
                if (location.HasValue)
                {
                    results.Add(new ImplementationInfo(d.Name, location.Value.File, location.Value.Line));
                }
            }
        }

        return JsonSerializer.Serialize(new ImplementationsResult(results));
    }
}
