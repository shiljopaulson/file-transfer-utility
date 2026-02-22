using System.Collections.Immutable;

using Sphere.FileTransfer.Models;
using Sphere.FileTransfer.Services.Models;

namespace Sphere.FileTransfer.Cli.Extensions;

internal static class SegregatedDirectoryExtensions
{
  extension(SegregatedDirectory segregationDirectory)
  {
    public bool IsAllFilesProcessed()
    {
      if (segregationDirectory is null || segregationDirectory.Files.Length == 0)
      {
        return false;
      }
      return segregationDirectory.Files.All(x => x.Status == FileStatus.Processed);
    }

    public bool HasAnyFilesProcessed()
    {
      if (segregationDirectory is null || segregationDirectory.Files.Length == 0)
      {
        return false;
      }
      return segregationDirectory.Files.Any(x => x.Status == FileStatus.Processed);
    }
  }

  public static bool IsAllFilesProcessed(this ImmutableArray<SegregatedDirectory> segregationDirectory)
  {
    return segregationDirectory.All(x => x.IsAllFilesProcessed());
  }

  public static bool HasAnyFilesProcessed(this ImmutableArray<SegregatedDirectory> segregationDirectory)
  {
    return segregationDirectory.All(x => x.HasAnyFilesProcessed());
  }
}
