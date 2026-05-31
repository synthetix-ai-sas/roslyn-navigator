namespace RoslynNavigator;

/// <summary>
/// Represents the current state of the Roslyn workspace.
/// </summary>
public enum WorkspaceState
{
    /// <summary>Workspace has not been initialized yet.</summary>
    NotStarted,

    /// <summary>Workspace is currently loading the solution and compiling projects.</summary>
    Loading,

    /// <summary>Workspace is ready to accept queries.</summary>
    Ready,

    /// <summary>Workspace failed to load. Check diagnostics for details.</summary>
    Error
}
