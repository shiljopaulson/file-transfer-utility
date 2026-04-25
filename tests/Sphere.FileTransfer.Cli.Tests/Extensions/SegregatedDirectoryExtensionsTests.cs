using System.Collections.Immutable;

using Sphere.FileTransfer.Cli.Extensions;
using Sphere.FileTransfer.Services.Models;

namespace Sphere.FileTransfer.Cli.Tests.Extensions;

public sealed class SegregatedDirectoryExtensionsTests
{
  private static SegregatedFile File(FileStatus status)
    => new() { File = new FileInfo("file.txt"), Status = status };

  private static SegregatedDirectory Dir(params FileStatus[] statuses)
    => new()
    {
      DirectoryPath = "/tmp",
      Files = statuses.Select(File).ToImmutableArray()
    };

  // ── Single-directory extension members ──────────────────────────────────

  [Fact]
  public void IsAllFilesProcessed_EmptyFiles_ReturnsFalse()
    => Assert.False(Dir().IsAllFilesProcessed());

  [Fact]
  public void IsAllFilesProcessed_AllProcessed_ReturnsTrue()
    => Assert.True(Dir(FileStatus.Processed, FileStatus.Processed).IsAllFilesProcessed());

  [Fact]
  public void IsAllFilesProcessed_OneUnprocessed_ReturnsFalse()
    => Assert.False(Dir(FileStatus.Processed, FileStatus.Unprocessed).IsAllFilesProcessed());

  [Fact]
  public void HasAnyFilesProcessed_EmptyFiles_ReturnsFalse()
    => Assert.False(Dir().HasAnyFilesProcessed());

  [Fact]
  public void HasAnyFilesProcessed_OneProcessed_ReturnsTrue()
    => Assert.True(Dir(FileStatus.Error, FileStatus.Processed).HasAnyFilesProcessed());

  [Fact]
  public void HasAnyFilesProcessed_AllErrors_ReturnsFalse()
    => Assert.False(Dir(FileStatus.Error, FileStatus.Error).HasAnyFilesProcessed());

  // ── Array extension methods ──────────────────────────────────────────────

  [Fact]
  public void IsAllFilesProcessed_Array_AllDirectoriesAllProcessed_ReturnsTrue()
  {
    ImmutableArray<SegregatedDirectory> dirs = [
      Dir(FileStatus.Processed),
      Dir(FileStatus.Processed)
    ];
    Assert.True(dirs.IsAllFilesProcessed());
  }

  [Fact]
  public void IsAllFilesProcessed_Array_OneDirectoryHasError_ReturnsFalse()
  {
    ImmutableArray<SegregatedDirectory> dirs = [
      Dir(FileStatus.Processed),
      Dir(FileStatus.Error)
    ];
    Assert.False(dirs.IsAllFilesProcessed());
  }

  [Fact]
  public void HasAnyFilesProcessed_Array_OneDirectoryHasProcessed_ReturnsTrue()
  {
    ImmutableArray<SegregatedDirectory> dirs = [
      Dir(FileStatus.Error),
      Dir(FileStatus.Processed)
    ];
    Assert.True(dirs.HasAnyFilesProcessed());
  }

  [Fact]
  public void HasAnyFilesProcessed_Array_NoDirectoryHasProcessed_ReturnsFalse()
  {
    ImmutableArray<SegregatedDirectory> dirs = [
      Dir(FileStatus.Error),
      Dir(FileStatus.Duplicate)
    ];
    Assert.False(dirs.HasAnyFilesProcessed());
  }
}