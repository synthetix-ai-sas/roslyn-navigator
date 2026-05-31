using System.ComponentModel;
using System.Text.Json;
using RoslynNavigator.Responses;
using Microsoft.CodeAnalysis;
using ModelContextProtocol.Server;

namespace RoslynNavigator.Tools;

[McpServerToolType]
public static class GetPublicApiTool
{
    [McpServerTool(Name = "get_public_api"), Description("Get the public members of a type without reading the full source file. Returns method signatures, properties, events.")]
    public static async Task<string> ExecuteAsync(
        WorkspaceManager workspace,
        [Description("The type name to get the public API for")] string typeName,
        CancellationToken ct = default)
    {
        var notReady = await workspace.EnsureReadyOrStatusAsync(ct);
        if (notReady is not null) return notReady;

        var symbol = await SymbolResolver.ResolveSymbolAsync(workspace, typeName, ct: ct);
        if (symbol is not INamedTypeSymbol typeSymbol)
            return JsonSerializer.Serialize(new PublicApiResult("not found", []));

        var members = typeSymbol.GetMembers()
            .Where(m => m.DeclaredAccessibility == Accessibility.Public)
            .Where(m => !m.IsImplicitlyDeclared) // Exclude compiler-generated members
            .Where(m => m is not IMethodSymbol { MethodKind: MethodKind.PropertyGet or MethodKind.PropertySet })
            .Select(m => new MemberInfo(
                Kind: GetMemberKind(m),
                Signature: m.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat),
                Accessibility: m.DeclaredAccessibility.ToString().ToLowerInvariant()))
            .ToList();

        var typeKind = SymbolResolver.GetKindString(typeSymbol);

        return JsonSerializer.Serialize(new PublicApiResult(typeKind, members));
    }

    private static string GetMemberKind(ISymbol symbol) => symbol switch
    {
        IMethodSymbol { MethodKind: MethodKind.Constructor } => "constructor",
        IMethodSymbol => "method",
        IPropertySymbol => "property",
        IFieldSymbol => "field",
        IEventSymbol => "event",
        INamedTypeSymbol => "nested type",
        _ => symbol.Kind.ToString().ToLowerInvariant()
    };
}
