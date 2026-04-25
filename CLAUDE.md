# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Commands

```bash
# Build
dotnet build

# Test (all)
dotnet test

# Test (single project)
dotnet test tests/Sphere.FileTransfer.Services.Tests/
dotnet test tests/Sphere.FileTransfer.Cli.Tests/

# Format (check only — runs automatically in pre-commit hook)
dotnet format --verify-no-changes

# Publish single-file executable
dotnet publish -c Release
```

The pre-commit hook runs `dotnet format --verify-no-changes` on staged `.cs` files. Formatting must pass before committing. If the hook fails with a restore error, run `dotnet restore` first.

`.editorconfig` sets `insert_final_newline = false` — files must not end with a trailing newline.

## Architecture

This is a CLI tool (`ftu`) for bulk file copy/move operations. It supports two modes: **delimited** (read filenames from a CSV/TSV/PSV file) and **pattern** (match files by wildcard glob).

**Solution layout:**

```
src/
  Sphere.FileTransfer.Models    # Domain models only (no logic)
  Sphere.FileTransfer.Services  # File operation business logic
  Sphere.FileTransfer.Cli       # CLI entry point, DI wiring, output formatting
tests/
  Sphere.FileTransfer.Services.Tests
  Sphere.FileTransfer.Cli.Tests
```

**Dependency graph:** `Cli → Services → Models`

### Key abstractions

- **`BaseFileOptions`** (`Models/`) — abstract base holding shared options: `Sources`, `Destination`, `Operation` (Copy/Move), `Overwrite`, `DryRun`. `DelimitedOptions` and `PatternOptions` extend it.

- **`BaseFileService<T>`** (`Services/`) — template method base for both service implementations; handles copy/move dispatch, overwrite logic, dry-run, cancellation, and exception handling.
  - `DelimitedService` — reads filenames from a delimited file via `IDelimitedReader`, resolves them from source directories via `IDirectoryReader`.
  - `PatternService` — discovers files matching a wildcard search pattern via `IDirectoryReader`.

- **`BaseCommand<T>`** (`Cli/Commands/`) — wraps `System.CommandLine` command construction. `DelimitedCommand` and `PatternCommand` extend it.

- **Handlers** (`Cli/Handlers/`) — bridge between CLI commands and services; map CLI options to domain options and format results.

- **Validators** (`Services/Validators/`) — FluentValidation-based validators for `DelimitedOptions` and `PatternOptions` with shared extension rules.

- **Result writers** (`Cli/Writer/`) — `IResultWriter<T>` with text and JSON implementations for output formatting.

### DI and configuration

`Program.cs` wires all services via `Microsoft.Extensions.DependencyInjection`. Serilog is configured from `ftu.appsettings.json` (console + rolling file sink, 7-day retention, default level Error).

### Code standards

- `TreatWarningsAsErrors` is enabled — all warnings are errors.
- Nullable reference types are enabled globally.
- SonarAnalyzer.CSharp runs on all projects.
- `.editorconfig` enforces 2-space indent and standard C# naming conventions (PascalCase types, camelCase locals).
- NuGet versions are centrally managed in `Directory.Packages.props`.
