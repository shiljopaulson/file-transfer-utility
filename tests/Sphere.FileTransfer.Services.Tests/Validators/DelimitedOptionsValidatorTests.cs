using System.Collections.Immutable;

using Sphere.FileTransfer.Models;
using Sphere.FileTransfer.Services.Validators;

namespace Sphere.FileTransfer.Services.Tests.Validators;

public sealed class DelimitedOptionsValidatorTests : IDisposable
{
  private readonly string _tempRoot;
  private readonly DirectoryInfo _sourceDir;
  private readonly DirectoryInfo _destinationDir;
  private readonly FileInfo _delimitedFile;

  public DelimitedOptionsValidatorTests()
  {
    _tempRoot = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
    _sourceDir = Directory.CreateDirectory(Path.Combine(_tempRoot, "source"));
    _destinationDir = Directory.CreateDirectory(Path.Combine(_tempRoot, "destination"));
    var filePath = Path.Combine(_tempRoot, "test.csv");
    File.WriteAllText(filePath, "col1,col2\nvalue1,value2");
    _delimitedFile = new FileInfo(filePath);
  }

  public void Dispose() => Directory.Delete(_tempRoot, recursive: true);

  private DelimitedOptions Build(
    ImmutableArray<DirectoryInfo>? sources = null,
    DirectoryInfo? destination = null,
    FileInfo? file = null,
    byte column = 1,
    char delimiter = ',')
  {
    return new DelimitedOptions(
      sources ?? [_sourceDir],
      destination ?? _destinationDir,
      Operation.Copy,
      false,
      false,
      file ?? _delimitedFile,
      column,
      false,
      delimiter);
  }

  [Fact]
  public async Task ValidOptions_IsValid()
  {
    var result = await new DelimitedOptionsValidator().ValidateAsync(Build(), TestContext.Current.CancellationToken);
    Assert.True(result.IsValid);
  }

  [Fact]
  public async Task NoSources_IsInvalid()
  {
    var result = await new DelimitedOptionsValidator().ValidateAsync(Build(sources: []), TestContext.Current.CancellationToken);
    Assert.False(result.IsValid);
  }

  [Fact]
  public async Task DuplicateSources_IsInvalid()
  {
    var result = await new DelimitedOptionsValidator().ValidateAsync(
      Build(sources: [_sourceDir, _sourceDir]), TestContext.Current.CancellationToken);
    Assert.False(result.IsValid);
  }

  [Fact]
  public async Task NonExistentSource_IsInvalid()
  {
    var result = await new DelimitedOptionsValidator().ValidateAsync(
      Build(sources: [new DirectoryInfo(Path.Combine(_tempRoot, "missing"))]), TestContext.Current.CancellationToken);
    Assert.False(result.IsValid);
  }

  [Fact]
  public async Task NonExistentDestination_IsInvalid()
  {
    var result = await new DelimitedOptionsValidator().ValidateAsync(
      Build(destination: new DirectoryInfo(Path.Combine(_tempRoot, "missing"))), TestContext.Current.CancellationToken);
    Assert.False(result.IsValid);
  }

  [Fact]
  public async Task NonExistentFile_IsInvalid()
  {
    var result = await new DelimitedOptionsValidator().ValidateAsync(
      Build(file: new FileInfo(Path.Combine(_tempRoot, "missing.csv"))), TestContext.Current.CancellationToken);
    Assert.False(result.IsValid);
  }

  [Fact]
  public async Task ColumnZero_IsInvalid()
  {
    var result = await new DelimitedOptionsValidator().ValidateAsync(Build(column: 0), TestContext.Current.CancellationToken);
    Assert.False(result.IsValid);
  }

  [Fact]
  public async Task ColumnOne_IsValid()
  {
    var result = await new DelimitedOptionsValidator().ValidateAsync(Build(column: 1), TestContext.Current.CancellationToken);
    Assert.True(result.IsValid);
  }

  [Fact]
  public async Task EmptyDelimiter_IsInvalid()
  {
    var result = await new DelimitedOptionsValidator().ValidateAsync(Build(delimiter: '\0'), TestContext.Current.CancellationToken);
    Assert.False(result.IsValid);
  }
}