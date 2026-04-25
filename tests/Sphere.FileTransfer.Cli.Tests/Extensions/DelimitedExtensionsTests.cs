using System.Collections.Immutable;

using Sphere.FileTransfer.Cli.Extensions;
using Sphere.FileTransfer.Services.Models;

namespace Sphere.FileTransfer.Cli.Tests.Extensions;

public sealed class DelimitedExtensionsTests
{
  private static DelimitedFile BuildFile(params LineStatus[] statuses)
  {
    var lines = statuses.Select((s, i) => new DelimitedFileLine { Number = i + 1, Status = s }).ToImmutableArray();
    return new DelimitedFile { FileFullName = "test.csv", Lines = lines };
  }

  [Fact]
  public void IsAllLinesProcessed_EmptyLines_ReturnsFalse()
    => Assert.False(BuildFile().IsAllLinesProcessed());

  [Fact]
  public void IsAllLinesProcessed_AllProcessed_ReturnsTrue()
    => Assert.True(BuildFile(LineStatus.Processed, LineStatus.Processed).IsAllLinesProcessed());

  [Fact]
  public void IsAllLinesProcessed_AllSkipped_ReturnsTrue()
    => Assert.True(BuildFile(LineStatus.Skipped, LineStatus.Skipped).IsAllLinesProcessed());

  [Fact]
  public void IsAllLinesProcessed_MixedProcessedAndSkipped_ReturnsTrue()
    => Assert.True(BuildFile(LineStatus.Skipped, LineStatus.Processed).IsAllLinesProcessed());

  [Fact]
  public void IsAllLinesProcessed_OneError_ReturnsFalse()
    => Assert.False(BuildFile(LineStatus.Processed, LineStatus.Error).IsAllLinesProcessed());

  [Fact]
  public void HasAnyLinesProcessed_EmptyLines_ReturnsFalse()
    => Assert.False(BuildFile().HasAnyLinesProcessed());

  [Fact]
  public void HasAnyLinesProcessed_OneProcessed_ReturnsTrue()
    => Assert.True(BuildFile(LineStatus.Error, LineStatus.Processed).HasAnyLinesProcessed());

  [Fact]
  public void HasAnyLinesProcessed_OneSkipped_ReturnsTrue()
    => Assert.True(BuildFile(LineStatus.Error, LineStatus.Skipped).HasAnyLinesProcessed());

  [Fact]
  public void HasAnyLinesProcessed_AllErrors_ReturnsFalse()
    => Assert.False(BuildFile(LineStatus.Error, LineStatus.Error).HasAnyLinesProcessed());

  [Fact]
  public void HasAnyLinesCanceled_EmptyLines_ReturnsFalse()
    => Assert.False(BuildFile().HasAnyLinesCanceled());

  [Fact]
  public void HasAnyLinesCanceled_OneCanceled_ReturnsTrue()
    => Assert.True(BuildFile(LineStatus.Processed, LineStatus.Canceled).HasAnyLinesCanceled());

  [Fact]
  public void HasAnyLinesCanceled_NoCanceled_ReturnsFalse()
    => Assert.False(BuildFile(LineStatus.Processed, LineStatus.Error).HasAnyLinesCanceled());
}