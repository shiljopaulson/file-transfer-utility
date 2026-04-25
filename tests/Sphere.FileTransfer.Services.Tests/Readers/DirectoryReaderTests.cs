using System.Collections.Immutable;

using Microsoft.Extensions.Logging.Abstractions;

using Sphere.FileTransfer.Services.Models;
using Sphere.FileTransfer.Services.Readers;

namespace Sphere.FileTransfer.Services.Tests.Readers;

public sealed class DirectoryReaderTests : IDisposable
{
  private readonly string _tempRoot;
  private readonly DirectoryReader _reader;

  public DirectoryReaderTests()
  {
    _tempRoot = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
    Directory.CreateDirectory(_tempRoot);
    _reader = new DirectoryReader(NullLogger<DirectoryReader>.Instance);
  }

  public void Dispose() => Directory.Delete(_tempRoot, recursive: true);

  private DirectoryInfo CreateDirWithFiles(string dirName, params string[] fileNames)
  {
    var dir = Directory.CreateDirectory(Path.Combine(_tempRoot, dirName));
    foreach (var name in fileNames)
    {
      File.WriteAllText(Path.Combine(dir.FullName, name), string.Empty);
    }
    return dir;
  }

  [Fact]
  public void Read_DirectoryWithFiles_ReturnsAllFiles()
  {
    var dir = CreateDirWithFiles("source", "a.txt", "b.txt", "c.txt");
    var result = _reader.Read([dir], "*.*", CancellationToken.None);
    Assert.Single(result);
    Assert.Equal(3, result[0].Files.Length);
    Assert.Equal(DirectoryStatus.Unprocessed, result[0].Status);
  }

  [Fact]
  public void Read_PatternFilter_ReturnsOnlyMatchingFiles()
  {
    var dir = CreateDirWithFiles("source", "a.txt", "b.png", "c.txt");
    var result = _reader.Read([dir], "*.txt", CancellationToken.None);
    Assert.Equal(2, result[0].Files.Length);
    Assert.All(result[0].Files, f => Assert.Equal(".txt", f.File.Extension));
  }

  [Fact]
  public void Read_EmptyDirectory_ReturnsNoMatchingFilesStatus()
  {
    var dir = Directory.CreateDirectory(Path.Combine(_tempRoot, "empty"));
    var result = _reader.Read([new DirectoryInfo(dir.FullName)], "*.*", CancellationToken.None);
    Assert.Equal(DirectoryStatus.NoMatchingFiles, result[0].Status);
  }

  [Fact]
  public void Read_NoPatternMatch_ReturnsNoMatchingFilesStatus()
  {
    var dir = CreateDirWithFiles("source", "a.txt", "b.txt");
    var result = _reader.Read([dir], "*.png", CancellationToken.None);
    Assert.Equal(DirectoryStatus.NoMatchingFiles, result[0].Status);
  }

  [Fact]
  public void Read_MultipleDirectories_ReturnsOneResultPerDirectory()
  {
    var dir1 = CreateDirWithFiles("source1", "a.txt");
    var dir2 = CreateDirWithFiles("source2", "b.txt", "c.txt");
    var result = _reader.Read([dir1, dir2], "*.*", CancellationToken.None);
    Assert.Equal(2, result.Length);
    Assert.Single(result[0].Files);
    Assert.Equal(2, result[1].Files.Length);
  }

  [Fact]
  public void Read_CancelledToken_ReturnsCanceledStatus()
  {
    var dir = CreateDirWithFiles("source", "a.txt");
    using var cts = new CancellationTokenSource();
    cts.Cancel();
    var result = _reader.Read([dir], "*.*", cts.Token);
    Assert.Equal(DirectoryStatus.Canceled, result[0].Status);
  }

  [Fact]
  public void Read_DirectoryPaths_MatchInputDirectories()
  {
    var dir = CreateDirWithFiles("source", "a.txt");
    var result = _reader.Read([dir], "*.*", CancellationToken.None);
    Assert.Equal(dir.FullName, result[0].DirectoryPath);
  }
}