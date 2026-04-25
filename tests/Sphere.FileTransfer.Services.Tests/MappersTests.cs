using Sphere.FileTransfer.Services.Models;

namespace Sphere.FileTransfer.Services.Tests;

public sealed class MappersTests
{
  [Theory]
  [InlineData(FileStatus.Canceled, LineStatus.Canceled)]
  [InlineData(FileStatus.Processed, LineStatus.Processed)]
  [InlineData(FileStatus.Unprocessed, LineStatus.Unprocessed)]
  [InlineData(FileStatus.Error, LineStatus.Error)]
  [InlineData(FileStatus.Duplicate, LineStatus.Error)]
  public void Map_FileStatusToLineStatus_ReturnsExpected(FileStatus input, LineStatus expected)
  {
    Assert.Equal(expected, Mappers.Map(input));
  }
}