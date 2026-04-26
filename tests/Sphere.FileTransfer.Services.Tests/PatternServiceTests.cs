using System.Collections.Immutable;

using FluentValidation;

using Microsoft.Extensions.Logging.Abstractions;

using Sphere.FileTransfer.Models;
using Sphere.FileTransfer.Services.Models;
using Sphere.FileTransfer.Services.Readers;
using Sphere.FileTransfer.Services.Validators;

namespace Sphere.FileTransfer.Services.Tests;

public sealed class PatternServiceTests : IDisposable
{
  private readonly string _tempRoot;
  private readonly string _sourceDir;
  private readonly string _destDir;

  public PatternServiceTests()
  {
    _tempRoot = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
    _sourceDir = Path.Combine(_tempRoot, "source");
    _destDir = Path.Combine(_tempRoot, "dest");
    Directory.CreateDirectory(_sourceDir);
    Directory.CreateDirectory(_destDir);
  }

  public void Dispose() => Directory.Delete(_tempRoot, recursive: true);

  private PatternOptions Options(bool dryRun = false, bool overwrite = false, string pattern = "*.txt", Operation operation = Operation.Copy)
    => new(
      Sources: [new DirectoryInfo(_sourceDir)],
      Destination: new DirectoryInfo(_destDir),
      operation: operation,
      Overwrite: overwrite,
      DryRun: dryRun,
      SearchPattern: pattern
    );

  private static PatternService CreateService(IDirectoryReader reader, bool realValidator = false)
    => new(
      reader,
      realValidator ? new PatternOptionsValidator() : (AbstractValidator<PatternOptions>)new PassValidator<PatternOptions>(),
      NullLogger<PatternService>.Instance
    );

  private SegregatedDirectory SourceDir(params string[] fileNames)
    => new()
    {
      DirectoryPath = _sourceDir,
      Files = fileNames
        .Select(name => new SegregatedFile { File = new FileInfo(Path.Combine(_sourceDir, name)) })
        .ToImmutableArray()
    };

  private sealed class StubDirectoryReader(ImmutableArray<SegregatedDirectory> result) : IDirectoryReader
  {
    public ImmutableArray<SegregatedDirectory> Read(ImmutableArray<DirectoryInfo> directories, string searchPattern, CancellationToken cancellationToken)
    {
      cancellationToken.ThrowIfCancellationRequested();
      return result;
    }
  }

  private sealed class PassValidator<T> : AbstractValidator<T> { }

  // ── Validation ────────────────────────────────────────────────────────────

  [Fact]
  public async Task Process_InvalidOptions_ReturnsEmptyArray()
  {
    var options = new PatternOptions(
      Sources: ImmutableArray<DirectoryInfo>.Empty,
      Destination: new DirectoryInfo(_destDir),
      operation: Operation.Copy,
      Overwrite: false,
      DryRun: false,
      SearchPattern: "*.txt"
    );
    var service = CreateService(new StubDirectoryReader([]), realValidator: true);
    var result = await service.Process(options, TestContext.Current.CancellationToken);
    Assert.Empty(result);
  }

  // ── Happy path ────────────────────────────────────────────────────────────

  [Fact]
  public async Task Process_MatchingFiles_CopiesFileAndReturnsProcessed()
  {
    const string fileName = "file.txt";
    await File.WriteAllTextAsync(Path.Combine(_sourceDir, fileName), "data", TestContext.Current.CancellationToken);
    var service = CreateService(new StubDirectoryReader([SourceDir(fileName)]));
    var result = await service.Process(Options(), TestContext.Current.CancellationToken);
    Assert.Single(result);
    Assert.Equal(FileStatus.Processed, result[0].Files[0].Status);
    Assert.True(File.Exists(Path.Combine(_destDir, fileName)));
  }

  // ── Duplicates ────────────────────────────────────────────────────────────

  [Fact]
  public async Task Process_DuplicateAcrossDirectories_ProcessesFirstAndMarksDuplicate()
  {
    const string fileName = "file.txt";
    var sourceDir2 = Path.Combine(_tempRoot, "source2");
    Directory.CreateDirectory(sourceDir2);
    await File.WriteAllTextAsync(Path.Combine(_sourceDir, fileName), "data1", TestContext.Current.CancellationToken);
    await File.WriteAllTextAsync(Path.Combine(sourceDir2, fileName), "data2", TestContext.Current.CancellationToken);
    var dir2 = new SegregatedDirectory
    {
      DirectoryPath = sourceDir2,
      Files = [new SegregatedFile { File = new FileInfo(Path.Combine(sourceDir2, fileName)) }]
    };
    var service = CreateService(new StubDirectoryReader([SourceDir(fileName), dir2]));
    var result = await service.Process(Options(), TestContext.Current.CancellationToken);
    Assert.Equal(2, result.Length);
    Assert.Equal(FileStatus.Processed, result[0].Files[0].Status);
    Assert.Equal(FileStatus.Duplicate, result[1].Files[0].Status);
  }

  // ── Dry-run ───────────────────────────────────────────────────────────────

  [Fact]
  public async Task Process_DryRun_FileNotCopied()
  {
    const string fileName = "file.txt";
    await File.WriteAllTextAsync(Path.Combine(_sourceDir, fileName), "data", TestContext.Current.CancellationToken);
    var service = CreateService(new StubDirectoryReader([SourceDir(fileName)]));
    var result = await service.Process(Options(dryRun: true), TestContext.Current.CancellationToken);
    Assert.Single(result);
    Assert.Equal(FileStatus.Unprocessed, result[0].Files[0].Status);
    Assert.False(File.Exists(Path.Combine(_destDir, fileName)));
  }

  // ── Destination exists ────────────────────────────────────────────────────

  [Fact]
  public async Task Process_DestinationExists_NoOverwrite_ReturnsFileError()
  {
    const string fileName = "file.txt";
    await File.WriteAllTextAsync(Path.Combine(_sourceDir, fileName), "source", TestContext.Current.CancellationToken);
    await File.WriteAllTextAsync(Path.Combine(_destDir, fileName), "existing", TestContext.Current.CancellationToken);
    var service = CreateService(new StubDirectoryReader([SourceDir(fileName)]));
    var result = await service.Process(Options(overwrite: false), TestContext.Current.CancellationToken);
    Assert.Single(result);
    Assert.Equal(FileStatus.Error, result[0].Files[0].Status);
  }

  [Fact]
  public async Task Process_DestinationExists_Overwrite_CopiesFile()
  {
    const string fileName = "file.txt";
    await File.WriteAllTextAsync(Path.Combine(_sourceDir, fileName), "new-content", TestContext.Current.CancellationToken);
    await File.WriteAllTextAsync(Path.Combine(_destDir, fileName), "old-content", TestContext.Current.CancellationToken);
    var service = CreateService(new StubDirectoryReader([SourceDir(fileName)]));
    var result = await service.Process(Options(overwrite: true), TestContext.Current.CancellationToken);
    Assert.Single(result);
    Assert.Equal(FileStatus.Processed, result[0].Files[0].Status);
    Assert.Equal("new-content", await File.ReadAllTextAsync(Path.Combine(_destDir, fileName), TestContext.Current.CancellationToken));
  }

  // ── Overwrite selects last duplicate ─────────────────────────────────────

  [Fact]
  public async Task Process_Overwrite_WithDuplicates_UsesLastOccurrence()
  {
    const string fileName = "file.txt";
    var sourceDir2 = Path.Combine(_tempRoot, "source2");
    Directory.CreateDirectory(sourceDir2);
    await File.WriteAllTextAsync(Path.Combine(_sourceDir, fileName), "first", TestContext.Current.CancellationToken);
    await File.WriteAllTextAsync(Path.Combine(sourceDir2, fileName), "last", TestContext.Current.CancellationToken);
    var dir2 = new SegregatedDirectory
    {
      DirectoryPath = sourceDir2,
      Files = [new SegregatedFile { File = new FileInfo(Path.Combine(sourceDir2, fileName)) }]
    };
    var service = CreateService(new StubDirectoryReader([SourceDir(fileName), dir2]));
    await service.Process(Options(overwrite: true), TestContext.Current.CancellationToken);
    Assert.Equal("last", await File.ReadAllTextAsync(Path.Combine(_destDir, fileName), TestContext.Current.CancellationToken));
  }

  // ── Cancellation ─────────────────────────────────────────────────────────

  [Fact]
  public async Task Process_CancelledToken_ThrowsOperationCanceledException()
  {
    using var cts = new CancellationTokenSource();
    await cts.CancelAsync();
    var service = CreateService(new StubDirectoryReader([]));
    await Assert.ThrowsAsync<OperationCanceledException>(() =>
      service.Process(Options(), cts.Token));
  }
}