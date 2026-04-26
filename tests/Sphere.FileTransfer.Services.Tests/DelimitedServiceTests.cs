using System.Collections.Immutable;

using FluentValidation;

using Microsoft.Extensions.Logging.Abstractions;

using Sphere.FileTransfer.Models;
using Sphere.FileTransfer.Services.Models;
using Sphere.FileTransfer.Services.Readers;
using Sphere.FileTransfer.Services.Validators;

namespace Sphere.FileTransfer.Services.Tests;

public sealed class DelimitedServiceTests : IDisposable
{
  private readonly string _tempRoot;
  private readonly string _sourceDir;
  private readonly string _destDir;
  private readonly string _csvFile;

  public DelimitedServiceTests()
  {
    _tempRoot = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
    _sourceDir = Path.Combine(_tempRoot, "source");
    _destDir = Path.Combine(_tempRoot, "dest");
    _csvFile = Path.Combine(_tempRoot, "files.csv");
    Directory.CreateDirectory(_sourceDir);
    Directory.CreateDirectory(_destDir);
    File.WriteAllText(_csvFile, string.Empty);
  }

  public void Dispose() => Directory.Delete(_tempRoot, recursive: true);

  private DelimitedOptions Options(bool dryRun = false, bool overwrite = false, byte column = 1, Operation operation = Operation.Copy)
    => new(
      Sources: [new DirectoryInfo(_sourceDir)],
      Destination: new DirectoryInfo(_destDir),
      Operation: operation,
      Overwrite: overwrite,
      DryRun: dryRun,
      File: new FileInfo(_csvFile),
      Column: column,
      NoHeader: true,
      Delimiter: ','
    );

  private static DelimitedService CreateService(IDelimitedReader reader, bool realValidator = false)
    => new(
      reader,
      realValidator ? new DelimitedOptionsValidator() : (AbstractValidator<DelimitedOptions>)new PassValidator<DelimitedOptions>(),
      NullLogger<DelimitedService>.Instance
    );

  private static DelimitedFile BuildResult(params string[] fileNames)
  {
    var lines = fileNames
      .Select((name, i) => new DelimitedFileLine { Number = i + 1, DelimitedFields = [name] })
      .ToImmutableArray();
    return new DelimitedFile { FileFullName = "stub.csv", Lines = lines };
  }

  private sealed class StubDelimitedReader(DelimitedFile result) : IDelimitedReader
  {
    public DelimitedFile Read(string fileFullName, char delimiter, bool hasHeader, CancellationToken cancellationToken)
    {
      cancellationToken.ThrowIfCancellationRequested();
      return result;
    }
  }

  private sealed class PassValidator<T> : AbstractValidator<T> { }

  // ── Validation ────────────────────────────────────────────────────────────

  [Fact]
  public async Task Process_InvalidOptions_ReturnsErrorFile()
  {
    var options = new DelimitedOptions(
      Sources: ImmutableArray<DirectoryInfo>.Empty,
      Destination: new DirectoryInfo(_destDir),
      Operation: Operation.Copy,
      Overwrite: false,
      DryRun: false,
      File: new FileInfo(_csvFile),
      Column: 1,
      NoHeader: true,
      Delimiter: ','
    );
    var service = CreateService(new StubDelimitedReader(BuildResult()), realValidator: true);
    var result = await service.Process(options, TestContext.Current.CancellationToken);
    Assert.NotNull(result);
    Assert.Equal(FileStatus.Error, result.Status);
  }

  // ── Empty lines ───────────────────────────────────────────────────────────

  [Fact]
  public async Task Process_EmptyLines_ReturnsEmptyDelimitedFile()
  {
    var service = CreateService(new StubDelimitedReader(BuildResult()));
    var result = await service.Process(Options(), TestContext.Current.CancellationToken);
    Assert.NotNull(result);
    Assert.Empty(result.Lines);
  }

  // ── Happy path ────────────────────────────────────────────────────────────

  [Fact]
  public async Task Process_FileFoundAtSource_CopiesFileAndReturnsProcessed()
  {
    const string fileName = "file.txt";
    await File.WriteAllTextAsync(Path.Combine(_sourceDir, fileName), "data", TestContext.Current.CancellationToken);
    var service = CreateService(new StubDelimitedReader(BuildResult(fileName)));
    var result = await service.Process(Options(), TestContext.Current.CancellationToken);
    Assert.NotNull(result);
    Assert.Equal(LineStatus.Processed, result.Lines[0].Status);
    Assert.True(File.Exists(Path.Combine(_destDir, fileName)));
  }

  [Fact]
  public async Task Process_FileNotInSource_ReturnsLineError()
  {
    var service = CreateService(new StubDelimitedReader(BuildResult("missing.txt")));
    var result = await service.Process(Options(), TestContext.Current.CancellationToken);
    Assert.NotNull(result);
    Assert.Equal(LineStatus.Error, result.Lines[0].Status);
  }

  // ── Duplicates ────────────────────────────────────────────────────────────

  [Fact]
  public async Task Process_DuplicateFileEntries_MarksSecondAsDuplicate()
  {
    const string fileName = "file.txt";
    await File.WriteAllTextAsync(Path.Combine(_sourceDir, fileName), "data", TestContext.Current.CancellationToken);
    var service = CreateService(new StubDelimitedReader(BuildResult(fileName, fileName)));
    var result = await service.Process(Options(), TestContext.Current.CancellationToken);
    Assert.NotNull(result);
    Assert.Equal(LineStatus.Processed, result.Lines[0].Status);
    Assert.Equal(LineStatus.Duplicate, result.Lines[1].Status);
  }

  // ── Column bounds ─────────────────────────────────────────────────────────

  [Fact]
  public async Task Process_ColumnOutOfRange_ReturnsLineError()
  {
    var stub = new StubDelimitedReader(new DelimitedFile
    {
      FileFullName = "stub.csv",
      Lines = [new DelimitedFileLine { Number = 1, DelimitedFields = ["only-one-field"] }]
    });
    var service = CreateService(stub);
    // column=2 → fieldIndex=1, but line only has 1 field
    var result = await service.Process(Options(column: 2), TestContext.Current.CancellationToken);
    Assert.NotNull(result);
    Assert.Equal(LineStatus.Error, result.Lines[0].Status);
  }

  // ── Dry-run ───────────────────────────────────────────────────────────────

  [Fact]
  public async Task Process_DryRun_FileNotCopied()
  {
    const string fileName = "file.txt";
    await File.WriteAllTextAsync(Path.Combine(_sourceDir, fileName), "data", TestContext.Current.CancellationToken);
    var service = CreateService(new StubDelimitedReader(BuildResult(fileName)));
    var result = await service.Process(Options(dryRun: true), TestContext.Current.CancellationToken);
    Assert.NotNull(result);
    Assert.Equal(LineStatus.Unprocessed, result.Lines[0].Status);
    Assert.False(File.Exists(Path.Combine(_destDir, fileName)));
  }

  // ── Destination exists ────────────────────────────────────────────────────

  [Fact]
  public async Task Process_DestinationExists_NoOverwrite_ReturnsLineError()
  {
    const string fileName = "file.txt";
    await File.WriteAllTextAsync(Path.Combine(_sourceDir, fileName), "source", TestContext.Current.CancellationToken);
    await File.WriteAllTextAsync(Path.Combine(_destDir, fileName), "existing", TestContext.Current.CancellationToken);
    var service = CreateService(new StubDelimitedReader(BuildResult(fileName)));
    var result = await service.Process(Options(overwrite: false), TestContext.Current.CancellationToken);
    Assert.NotNull(result);
    Assert.Equal(LineStatus.Error, result.Lines[0].Status);
  }

  [Fact]
  public async Task Process_DestinationExists_Overwrite_ReturnsProcessed()
  {
    const string fileName = "file.txt";
    await File.WriteAllTextAsync(Path.Combine(_sourceDir, fileName), "new-content", TestContext.Current.CancellationToken);
    await File.WriteAllTextAsync(Path.Combine(_destDir, fileName), "old-content", TestContext.Current.CancellationToken);
    var service = CreateService(new StubDelimitedReader(BuildResult(fileName)));
    var result = await service.Process(Options(overwrite: true), TestContext.Current.CancellationToken);
    Assert.NotNull(result);
    Assert.Equal(LineStatus.Processed, result.Lines[0].Status);
  }

  // ── Cancellation ─────────────────────────────────────────────────────────

  [Fact]
  public async Task Process_CancelledToken_ThrowsOperationCanceledException()
  {
    using var cts = new CancellationTokenSource();
    await cts.CancelAsync();
    var service = CreateService(new StubDelimitedReader(BuildResult("file.txt")));
    await Assert.ThrowsAsync<OperationCanceledException>(() =>
      service.Process(Options(), cts.Token));
  }
}