using System.Collections.Immutable;

using Sphere.FileTransfer.Models;
using Sphere.FileTransfer.Services.Validators;

namespace Sphere.FileTransfer.Services.Tests.Validators;

public sealed class PatternOptionsValidatorTests : IDisposable
{
  private readonly string _tempRoot;
  private readonly DirectoryInfo _sourceDir;
  private readonly DirectoryInfo _destinationDir;

  public PatternOptionsValidatorTests()
  {
    _tempRoot = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
    _sourceDir = Directory.CreateDirectory(Path.Combine(_tempRoot, "source"));
    _destinationDir = Directory.CreateDirectory(Path.Combine(_tempRoot, "destination"));
  }

  public void Dispose() => Directory.Delete(_tempRoot, recursive: true);

  private PatternOptions Build(
    ImmutableArray<DirectoryInfo>? sources = null,
    DirectoryInfo? destination = null,
    string searchPattern = "*.*")
  {
    return new PatternOptions(
      sources ?? [_sourceDir],
      destination ?? _destinationDir,
      Operation.Copy,
      false,
      false,
      searchPattern);
  }

  [Fact]
  public async Task ValidOptions_IsValid()
  {
    var result = await new PatternOptionsValidator().ValidateAsync(Build(), TestContext.Current.CancellationToken);
    Assert.True(result.IsValid);
  }

  [Fact]
  public async Task NoSources_IsInvalid()
  {
    var result = await new PatternOptionsValidator().ValidateAsync(Build(sources: []), TestContext.Current.CancellationToken);
    Assert.False(result.IsValid);
  }

  [Fact]
  public async Task DuplicateSources_IsInvalid()
  {
    var result = await new PatternOptionsValidator().ValidateAsync(
      Build(sources: [_sourceDir, _sourceDir]), TestContext.Current.CancellationToken);
    Assert.False(result.IsValid);
  }

  [Fact]
  public async Task NonExistentSource_IsInvalid()
  {
    var result = await new PatternOptionsValidator().ValidateAsync(
      Build(sources: [new DirectoryInfo(Path.Combine(_tempRoot, "missing"))]), TestContext.Current.CancellationToken);
    Assert.False(result.IsValid);
  }

  [Fact]
  public async Task NonExistentDestination_IsInvalid()
  {
    var result = await new PatternOptionsValidator().ValidateAsync(
      Build(destination: new DirectoryInfo(Path.Combine(_tempRoot, "missing"))), TestContext.Current.CancellationToken);
    Assert.False(result.IsValid);
  }

  [Fact]
  public async Task EmptySearchPattern_IsInvalid()
  {
    var result = await new PatternOptionsValidator().ValidateAsync(Build(searchPattern: ""), TestContext.Current.CancellationToken);
    Assert.False(result.IsValid);
  }

  [Fact]
  public async Task SpecificExtensionPattern_IsValid()
  {
    var result = await new PatternOptionsValidator().ValidateAsync(Build(searchPattern: "*.png"), TestContext.Current.CancellationToken);
    Assert.True(result.IsValid);
  }
}