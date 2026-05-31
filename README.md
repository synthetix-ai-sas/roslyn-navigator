# RoslynNavigator — Roslyn MCP Server

> Token-efficient .NET codebase navigation via Roslyn semantic analysis.

## Overview

RoslynNavigator is a Model Context Protocol (MCP) server that provides Claude Code with semantic understanding of .NET solutions. Instead of reading entire source files (hundreds of tokens), Claude can query for specific symbols, references, and type hierarchies (tens of tokens).

## Prerequisites

- .NET 10 SDK
- A .NET solution file (`.sln` or `.slnx`)

## Tools

| Tool | Description |
|------|-------------|
| `find_symbol` | Find where a type, method, or property is defined |
| `find_references` | All usages of a symbol across the solution |
| `find_implementations` | Types that implement an interface or derive from a base class |
| `find_callers` | All methods that call a specific method |
| `find_overrides` | Overrides of a virtual or abstract method |
| `find_dead_code` | Unused types, methods, and properties |
| `get_type_hierarchy` | Inheritance chain, interfaces, and derived types |
| `get_public_api` | Public members of a type without reading the full file |
| `get_symbol_detail` | Full signature, parameters, return type, and XML docs |
| `get_project_graph` | Solution project dependency tree |
| `get_dependency_graph` | Call dependency graph for a method |
| `get_diagnostics` | Compiler and analyzer warnings/errors |
| `get_test_coverage_map` | Heuristic test coverage by naming convention |
| `detect_antipatterns` | .NET anti-patterns (async void, sync-over-async, etc.) |
| `detect_circular_dependencies` | Circular dependency detection at project or type level |

## Installation

> **Note:** `RoslynNavigator` is hosted on **GitHub Packages** (not the public NuGet registry). Steps 1–3 are a one-time setup per machine.

### Step 1 — Create a GitHub Personal Access Token (PAT)

1. Go to [GitHub → Settings → Developer settings → Personal access tokens → Tokens (classic)](https://github.com/settings/tokens/new)
2. Give it a descriptive name (e.g. `synthetix-ai-nuget-read`)
3. Select the scope **`read:packages`**
4. Click **Generate token** and copy it — you won't see it again

### Step 2 — Authorize the token for the Synthetix AI org (SSO)

Synthetix AI uses SAML SSO, so your PAT must be explicitly authorized for the organization — even if it already has the `read:packages` scope.

1. Go to [github.com/settings/tokens](https://github.com/settings/tokens)
2. Click on your token
3. Click **Configure SSO** next to the token
4. Click **Authorize** for `Synthetix AI`

> If you skip this step, `dotnet tool install` will fail with a **403 Forbidden** error.

### Step 3 — Add the GitHub Packages source (once per machine)

```bash
dotnet nuget add source https://nuget.pkg.github.com/synthetix-ai-sas/index.json \
  --name github-synthetix-ai \
  --username YOUR_GITHUB_USERNAME \
  --password YOUR_GITHUB_PAT \
  --store-password-in-clear-text
```

> Replace `YOUR_GITHUB_USERNAME` with your GitHub handle and `YOUR_GITHUB_PAT` with the token from Step 1.
>
> The `--store-password-in-clear-text` flag is required because NuGet on non-Windows platforms cannot use the encrypted credential store. The token is already scoped to read-only access, so this is acceptable.

The source is saved globally in your NuGet config (`~/.nuget/NuGet/NuGet.Config` on macOS/Linux, `%appdata%\NuGet\NuGet.Config` on Windows).

### Step 4 — Install the global tool

```bash
dotnet tool install -g RoslynNavigator
```

### Step 5 — Register with Claude Code

```bash
# Auto-discovers the solution from workspace roots — no --solution needed!
claude mcp add --scope user synthetix-ai-roslyn-navigator -- synthetix-ai-roslyn-navigator
```

You can also add it manually to your Claude Code global settings (`~/.claude/settings.json`):

```json
{
  "mcpServers": {
    "synthetix-ai-roslyn-navigator": {
      "command": "synthetix-ai-roslyn-navigator"
    }
  }
}
```

**Optional override**: Pass `--solution <path>` to specify a solution file or directory explicitly:

```json
{
  "mcpServers": {
    "synthetix-ai-roslyn-navigator": {
      "command": "synthetix-ai-roslyn-navigator",
      "args": ["--solution", "${workspaceFolder}"]
    }
  }
}
```

---

### As a Local Tool (per-repo)

```bash
dotnet new tool-manifest   # if you don't have one
dotnet tool install RoslynNavigator
```

Then add to your project's `.mcp.json`:

```json
{
  "mcpServers": {
    "synthetix-ai-roslyn-navigator": {
      "command": "dotnet",
      "args": ["tool", "run", "synthetix-ai-roslyn-navigator", "--", "--solution", "${workspaceFolder}"]
    }
  }
}
```

---
### Register with Copilot CLI

```bash
copilot mcp add synthetix-ai-roslyn-navigator -- synthetix-ai-roslyn-navigator
```
---

### GitHub Copilot (VS Code)

Requires VS Code 1.99+ with the GitHub Copilot extension.

**Global tool** — add to `.vscode/mcp.json` in your workspace (or to VS Code user settings under `mcp.servers`):

```json
{
  "servers": {
    "synthetix-ai-roslyn-navigator": {
      "type": "stdio",
      "command": "synthetix-ai-roslyn-navigator"
    }
  }
}
```

The server auto-discovers the solution from the workspace folder. No extra arguments needed.

**Optional override** — specify the solution explicitly:

```json
{
  "servers": {
    "synthetix-ai-roslyn-navigator": {
      "type": "stdio",
      "command": "synthetix-ai-roslyn-navigator",
      "args": ["--solution", "${workspaceFolder}"]
    }
  }
}
```

**Local tool** (per-repo, using `dotnet tool run`):

```json
{
  "servers": {
    "synthetix-ai-roslyn-navigator": {
      "type": "stdio",
      "command": "dotnet",
      "args": ["tool", "run", "synthetix-ai-roslyn-navigator", "--", "--solution", "${workspaceFolder}"]
    }
  }
}
```

---

### From Source (for contributors)

```bash
dotnet run --project mcp/RoslynNavigator/src/RoslynNavigator.csproj -- --solution /path/to/your/Solution.sln
```

## Solution Discovery

The server resolves the solution file in this order:

1. **Explicit `--solution` argument** — Pass a `.sln`/`.slnx` file path directly, or a directory to scan recursively
2. **Working directory scan** — If no argument, scans the current working directory recursively for solution files
3. **MCP roots discovery** — On the first tool call, if no solution was found at startup, the server requests workspace roots from the MCP host (e.g., Claude Code) and scans those directories. This is a one-shot attempt — if no solution is found, it won't retry. This enables true zero-arg global tool operation.
4. **Deterministic selection** — Shallowest solution wins (BFS); within the same depth, alphabetical (case-insensitive) ordering is used

### Recursive Search

Discovery searches up to **3 levels deep** using breadth-first search, so a solution at `src/MyApp.sln` or `src/backend/Api/Api.sln` is found automatically.

The following directories are skipped during scanning: `.git`, `.vs`, `.idea`, `node_modules`, `bin`, `obj`, `packages`, `artifacts`, `TestResults`, `.claude`.

## Architecture

```
Program.cs              → MSBuildLocator → Host → MCP stdio transport
WorkspaceManager.cs     → MSBuildWorkspace lifecycle, file watching, compilation caching
WorkspaceInitializer.cs → BackgroundService triggers workspace load on startup
SolutionDiscovery.cs    → Auto-detect .sln/.slnx from args or working directory
SymbolResolver.cs       → Cross-project symbol resolution with disambiguation
Tools/                  → MCP tool implementations (15 read-only tools)
Responses/              → Token-optimized JSON response DTOs
```

## Scaling

| Solution Size | Strategy |
|---|---|
| Small (1-15 projects) | Load entire workspace on startup, warm compilations in parallel (4 concurrent) |
| Large (15-50 projects) | Lazy-load compilations on first query per project with LRU cache (30 max) |
| Enterprise (50+) | Lazy loading + LRU eviction + warn if query touches unloaded project |

## Development

```bash
# Build
dotnet build mcp/RoslynNavigator/RoslynNavigator.slnx

# Run tests
dotnet test mcp/RoslynNavigator/RoslynNavigator.slnx

# Run manually against a directory
dotnet run --project mcp/RoslynNavigator/src/RoslynNavigator.csproj -- --solution /path/to/your/project/

# Run manually against a solution file
dotnet run --project mcp/RoslynNavigator/src/RoslynNavigator.csproj -- --solution /path/to/your/Solution.sln
```

## Changelog

### 0.7.0

- **Performance optimizations across all tools:**
  - `find_references` — Document text caching (200 async calls → ~10) + `maxResults` cap (default 100)
  - `find_dead_code` — Fast name-based pre-filter skips ~80-90% of expensive Roslyn reference searches
  - `get_dependency_graph` — O(1) file-to-project lookup via pre-built dictionary
  - `detect_circular_dependencies` — Reduced `ToDisplayString()` allocations with `IsUserType()` helper
  - `SymbolResolver` — `SymbolEqualityComparer.Default` for dedup instead of string allocation
  - Parallel compilation warming (`Parallel.ForEachAsync`, max 4 concurrent) for ~2-4x faster startup
  - Consolidated 4 duplicate `MakeRelativePath` into shared `SymbolResolver.MakeRelativePath`

### 0.6.0

- **MCP roots discovery** — When no solution is found at startup, tools now request workspace roots from the MCP host on the first call and auto-discover the solution. One-shot, thread-safe attempt via `EnsureReadyOrStatusAsync`.
- **Project restructured** — Source moved to `src/` and `tests/` layout with a new `.slnx` solution file.
- **Unified readiness check** — All 15 tools use `EnsureReadyOrStatusAsync` instead of inline state checks, reducing boilerplate and ensuring consistent lazy-init behavior.

### 0.5.2

- Recursive solution discovery (BFS up to 3 levels deep).

### 0.5.1

- Expanded README with installation, architecture, and scaling docs.

### 0.5.0

- Initial NuGet release as a `dotnet tool`. 15 read-only Roslyn MCP tools.
