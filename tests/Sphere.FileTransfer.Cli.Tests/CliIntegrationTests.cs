#pragma warning disable xUnit1051 // InvokeAsync has no CancellationToken overload in System.CommandLine 2.0
using System.Collections.Immutable;
using System.CommandLine;

using FluentValidation;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Sphere.FileTransfer.Cli.Commands;
using Sphere.FileTransfer.Cli.Handlers;
using Sphere.FileTransfer.Cli.Mappers;
using Sphere.FileTransfer.Cli.Models;
using Sphere.FileTransfer.Cli.Writer;
using Sphere.FileTransfer.Models;
using Sphere.FileTransfer.Services;
using Sphere.FileTransfer.Services.Models;
using Sphere.FileTransfer.Services.Readers;
using Sphere.FileTransfer.Services.Validators;

namespace Sphere.FileTransfer.Cli.Tests;

public sealed class CliIntegrationTests : IDisposable
{
  private readonly string _tempRoot;
  private readonly string _sourceDir;
  private readonly string _destDir;

  public CliIntegrationTests()
  {
    _tempRoot = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
    _sourceDir = Path.Combine(_tempRoot, "source");
    _destDir = Path.Combine(_tempRoot, "dest");
    Directory.CreateDirectory(_sourceDir);
    Directory.CreateDirectory(_destDir);
  }

  public void Dispose() => Directory.Delete(_tempRoot, recursive: true);

  private static RootCommand BuildRootCommand()
  {
    var services = new ServiceCollection();
    services.AddLogging();
    services.AddSingleton<IDelimitedService, DelimitedService>();
    services.AddSingleton<IPatternService, PatternService>();
    services.AddSingleton<IDelimitedReader, DelimitedReader>();
    services.AddSingleton<IDirectoryReader, DirectoryReader>();
    services.AddSingleton<AbstractValidator<DelimitedOptions>, DelimitedOptionsValidator>();
    services.AddSingleton<AbstractValidator<PatternOptions>, PatternOptionsValidator>();
    services.AddSingleton<DelimitedCommand>();
    services.AddSingleton<PatternCommand>();
    services.AddSingleton<RootHandler>();
    services.AddSingleton<DelimitedHandler>();
    services.AddSingleton<PatternHandler>();
    services.AddSingleton<IMap<Delimiter, char>, DelimiterToChar>();
    services.AddSingleton<IMap<char, Delimiter>, CharToDelimiter>();
    services.AddSingleton<IOptionsMapper<DelimitedOptions>, DelimitedOptionsMapper>();
    services.AddSingleton<IOptionsMapper<PatternOptions>, PatternOptionsMapper>();
    services.AddSingleton<IResultWriter<DelimitedFile>, DelimitedResultWriter>();
    services.AddSingleton<IResultWriter<ImmutableArray<SegregatedDirectory>>, PatternResultWriter>();
    services.AddSingleton<CliBuilder>();
    return services.BuildServiceProvider().GetRequiredService<CliBuilder>().Build();
  }

  // ── delimited ─────────────────────────────────────────────────────────────

  [Fact]
  public async Task Delimited_ValidCsv_CopiesFileAndReturnsSuccess()
  {
    const string fileName = "file.txt";
    await File.WriteAllTextAsync(Path.Combine(_sourceDir, fileName), "data", TestContext.Current.CancellationToken);
    var csvPath = Path.Combine(_tempRoot, "list.csv");
    await File.WriteAllTextAsync(csvPath, fileName, TestContext.Current.CancellationToken);
    var exitCode = await BuildRootCommand().Parse(
      ["delimited", "--file", csvPath, "--sources", _sourceDir, "--destination", _destDir, "--no-header"])
      .InvokeAsync();
    Assert.Equal(0, exitCode);
    Assert.True(File.Exists(Path.Combine(_destDir, fileName)));
  }

  [Fact]
  public async Task Delimited_DryRun_FileNotCopied()
  {
    const string fileName = "file.txt";
    await File.WriteAllTextAsync(Path.Combine(_sourceDir, fileName), "data", TestContext.Current.CancellationToken);
    var csvPath = Path.Combine(_tempRoot, "list.csv");
    await File.WriteAllTextAsync(csvPath, fileName, TestContext.Current.CancellationToken);
    var exitCode = await BuildRootCommand().Parse(
      ["delimited", "--file", csvPath, "--sources", _sourceDir, "--destination", _destDir, "--no-header", "--dry-run"])
      .InvokeAsync();
    Assert.Equal(0, exitCode);
    Assert.False(File.Exists(Path.Combine(_destDir, fileName)));
  }

  [Fact]
  public async Task Delimited_FileNotInSource_ReturnsError()
  {
    await File.WriteAllTextAsync(Path.Combine(_sourceDir, "other.txt"), "x", TestContext.Current.CancellationToken);
    var csvPath = Path.Combine(_tempRoot, "list.csv");
    await File.WriteAllTextAsync(csvPath, "missing.txt", TestContext.Current.CancellationToken);
    var exitCode = await BuildRootCommand().Parse(
      ["delimited", "--file", csvPath, "--sources", _sourceDir, "--destination", _destDir, "--no-header"])
      .InvokeAsync();
    Assert.NotEqual(0, exitCode);
  }

  [Fact]
  public async Task Delimited_NonExistentDestination_ReturnsParseError()
  {
    const string fileName = "file.txt";
    await File.WriteAllTextAsync(Path.Combine(_sourceDir, fileName), "data", TestContext.Current.CancellationToken);
    var csvPath = Path.Combine(_tempRoot, "list.csv");
    await File.WriteAllTextAsync(csvPath, fileName, TestContext.Current.CancellationToken);
    var fakeDest = Path.Combine(_tempRoot, "no-such-dir");
    var exitCode = await BuildRootCommand().Parse(
      ["delimited", "--file", csvPath, "--sources", _sourceDir, "--destination", fakeDest, "--no-header"])
      .InvokeAsync();
    Assert.NotEqual(0, exitCode);
  }

  [Fact]
  public async Task Delimited_MoveOperation_FileMovedFromSource()
  {
    const string fileName = "file.txt";
    await File.WriteAllTextAsync(Path.Combine(_sourceDir, fileName), "data", TestContext.Current.CancellationToken);
    var csvPath = Path.Combine(_tempRoot, "list.csv");
    await File.WriteAllTextAsync(csvPath, fileName, TestContext.Current.CancellationToken);
    var exitCode = await BuildRootCommand().Parse(
      ["delimited", "--file", csvPath, "--sources", _sourceDir, "--destination", _destDir, "--no-header", "--operation", "move"])
      .InvokeAsync();
    Assert.Equal(0, exitCode);
    Assert.True(File.Exists(Path.Combine(_destDir, fileName)));
    Assert.False(File.Exists(Path.Combine(_sourceDir, fileName)));
  }

  [Fact]
  public async Task Delimited_OverwriteExistingFile_ReturnsSuccess()
  {
    const string fileName = "file.txt";
    await File.WriteAllTextAsync(Path.Combine(_sourceDir, fileName), "new", TestContext.Current.CancellationToken);
    await File.WriteAllTextAsync(Path.Combine(_destDir, fileName), "old", TestContext.Current.CancellationToken);
    var csvPath = Path.Combine(_tempRoot, "list.csv");
    await File.WriteAllTextAsync(csvPath, fileName, TestContext.Current.CancellationToken);
    var exitCode = await BuildRootCommand().Parse(
      ["delimited", "--file", csvPath, "--sources", _sourceDir, "--destination", _destDir, "--no-header", "--overwrite"])
      .InvokeAsync();
    Assert.Equal(0, exitCode);
  }

  [Fact]
  public async Task Delimited_JsonOutputFormat_ReturnsSuccess()
  {
    const string fileName = "file.txt";
    await File.WriteAllTextAsync(Path.Combine(_sourceDir, fileName), "data", TestContext.Current.CancellationToken);
    var csvPath = Path.Combine(_tempRoot, "list.csv");
    await File.WriteAllTextAsync(csvPath, fileName, TestContext.Current.CancellationToken);
    var exitCode = await BuildRootCommand().Parse(
      ["delimited", "--file", csvPath, "--sources", _sourceDir, "--destination", _destDir, "--no-header", "--output-format", "json"])
      .InvokeAsync();
    Assert.Equal(0, exitCode);
  }

  // ── pattern ───────────────────────────────────────────────────────────────

  [Fact]
  public async Task Pattern_ValidPattern_CopiesMatchingFiles()
  {
    await File.WriteAllTextAsync(Path.Combine(_sourceDir, "file.txt"), "data", TestContext.Current.CancellationToken);
    var exitCode = await BuildRootCommand().Parse(
      ["pattern", "--search-pattern", "*.txt", "--sources", _sourceDir, "--destination", _destDir])
      .InvokeAsync();
    Assert.Equal(0, exitCode);
    Assert.True(File.Exists(Path.Combine(_destDir, "file.txt")));
  }

  [Fact]
  public async Task Pattern_DryRun_FileNotCopied()
  {
    await File.WriteAllTextAsync(Path.Combine(_sourceDir, "file.txt"), "data", TestContext.Current.CancellationToken);
    var exitCode = await BuildRootCommand().Parse(
      ["pattern", "--search-pattern", "*.txt", "--sources", _sourceDir, "--destination", _destDir, "--dry-run"])
      .InvokeAsync();
    Assert.Equal(0, exitCode);
    Assert.False(File.Exists(Path.Combine(_destDir, "file.txt")));
  }

  [Fact]
  public async Task Pattern_NoMatchingFiles_ReturnsError()
  {
    await File.WriteAllTextAsync(Path.Combine(_sourceDir, "file.txt"), "data", TestContext.Current.CancellationToken);
    var exitCode = await BuildRootCommand().Parse(
      ["pattern", "--search-pattern", "*.png", "--sources", _sourceDir, "--destination", _destDir])
      .InvokeAsync();
    Assert.NotEqual(0, exitCode);
  }

  [Fact]
  public async Task Pattern_OverwriteExistingFile_ReturnsSuccess()
  {
    await File.WriteAllTextAsync(Path.Combine(_sourceDir, "file.txt"), "new", TestContext.Current.CancellationToken);
    await File.WriteAllTextAsync(Path.Combine(_destDir, "file.txt"), "old", TestContext.Current.CancellationToken);
    var exitCode = await BuildRootCommand().Parse(
      ["pattern", "--search-pattern", "*.txt", "--sources", _sourceDir, "--destination", _destDir, "--overwrite"])
      .InvokeAsync();
    Assert.Equal(0, exitCode);
  }

  [Fact]
  public async Task Pattern_JsonOutputFormat_ReturnsSuccess()
  {
    await File.WriteAllTextAsync(Path.Combine(_sourceDir, "file.txt"), "data", TestContext.Current.CancellationToken);
    var exitCode = await BuildRootCommand().Parse(
      ["pattern", "--search-pattern", "*.txt", "--sources", _sourceDir, "--destination", _destDir, "--output-format", "json"])
      .InvokeAsync();
    Assert.Equal(0, exitCode);
  }

  // ── root ──────────────────────────────────────────────────────────────────

  [Fact]
  public async Task Root_Info_ReturnsSuccess()
  {
    var exitCode = await BuildRootCommand().Parse(["--info"]).InvokeAsync();
    Assert.Equal(0, exitCode);
  }
}