using System.ComponentModel;
using System.Text.Json;
using RoslynNavigator.Responses;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.FindSymbols;
using ModelContextProtocol.Server;

namespace RoslynNavigator.Tools;

[McpServerToolType]
public static class GetTypeHierarchyTool
{
    [McpServerTool(Name = "get_type_hierarchy"), Description("Get the full inheritance chain, interfaces, and derived types for a type.")]
    public static async Task<string> ExecuteAsync(
        WorkspaceManager workspace,
        [Description("The type name to get the hierarchy for")] string typeName,
        CancellationToken ct = default)
    {
        var notReady = await workspace.EnsureReadyOrStatusAsync(ct);
        if (notReady is not null) return notReady;

        var solution = workspace.GetSolution();
        if (solution is null)
            return JsonSerializer.Serialize(new TypeHierarchyResult([], [], []));

        var symbol = await SymbolResolver.ResolveSymbolAsync(workspace, typeName, ct: ct);
        if (symbol is not INamedTypeSymbol typeSymbol)
            return JsonSerializer.Serialize(new TypeHierarchyResult([], [], []));

        // Get base types chain
        var baseTypes = new List<string>();
        var current = typeSymbol.BaseType;
        while (current is not null && current.SpecialType != SpecialType.System_Object)
        {
            baseTypes.Add(current.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat));
            current = current.BaseType;
        }

        // Get interfaces
        var interfaces = typeSymbol.AllInterfaces
            .Select(i => i.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat))
            .ToList();

        // Get derived types
        var derivedTypes = new List<string>();
        var derived = await SymbolFinder.FindDerivedClassesAsync(typeSymbol, solution, cancellationToken: ct);
        foreach (var d in derived)
        {
            derivedTypes.Add(d.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat));
        }

        return JsonSerializer.Serialize(new TypeHierarchyResult(baseTypes, interfaces, derivedTypes));
    }
}
