using System.ComponentModel;
using System.Text.Json;
using System.Xml.Linq;
using RoslynNavigator.Responses;
using ModelContextProtocol.Server;

namespace RoslynNavigator.Tools;

[McpServerToolType]
public static class GetProjectGraphTool
{
    [McpServerTool(Name = "get_project_graph"), Description("Get the solution project dependency tree with names, paths, target frameworks, and project references.")]
    public static async Task<string> ExecuteAsync(
        WorkspaceManager workspace,
        CancellationToken ct = default)
    {
        var notReady = await workspace.EnsureReadyOrStatusAsync(ct);
        if (notReady is not null) return notReady;

        var solution = workspace.GetSolution();
        if (solution is null)
            return JsonSerializer.Serialize(new ProjectGraphResult("unknown", []));

        var projects = solution.Projects.Select(project =>
        {
            var references = project.ProjectReferences
                .Select(r => solution.GetProject(r.ProjectId)?.Name ?? "unknown")
                .ToList();

            var targetFramework = DetectTargetFramework(project);

            return new ProjectInfo(
                Name: project.Name,
                Path: project.FilePath ?? "unknown",
                TargetFramework: targetFramework,
                References: references);
        }).ToList();

        var solutionName = solution.FilePath is not null
            ? Path.GetFileName(solution.FilePath)
            : "unknown";

        return JsonSerializer.Serialize(new ProjectGraphResult(solutionName, projects));
    }

    private static string DetectTargetFramework(Microsoft.CodeAnalysis.Project project)
    {
        // Strategy 1: Parse from .csproj file (most reliable)
        if (project.FilePath is not null && File.Exists(project.FilePath))
        {
            try
            {
                var doc = XDocument.Load(project.FilePath);
                var tfm = doc.Root?.Descendants("TargetFramework").FirstOrDefault()?.Value;
                if (!string.IsNullOrEmpty(tfm))
                    return tfm;

                // Multi-target: return first framework from TargetFrameworks
                var tfms = doc.Root?.Descendants("TargetFrameworks").FirstOrDefault()?.Value;
                if (!string.IsNullOrEmpty(tfms))
                    return tfms.Split(';', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault() ?? "unknown";
            }
            catch
            {
                // Fall through to next strategy
            }
        }

        // Strategy 2: Check preprocessor symbols (e.g., NET10_0, NET8_0)
        var preprocessorSymbol = project.ParseOptions?.PreprocessorSymbolNames
            .Where(s => s.StartsWith("NET"))
            .OrderByDescending(s => s.Length)
            .FirstOrDefault();

        if (preprocessorSymbol is not null)
        {
            // Convert "NET10_0" to "net10.0", "NET8_0_OR_GREATER" stays as-is but we prefer exact match
            var exact = preprocessorSymbol.Replace("_OR_GREATER", "");
            return exact.ToLowerInvariant().Replace('_', '.');
        }

        return "unknown";
    }
}
