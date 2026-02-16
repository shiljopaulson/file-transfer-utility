using Sphere.FileTransfer.Models;
using Sphere.FileTransfer.Services.Models;

namespace Sphere.FileTransfer.Cli.Extensions;

public static class SegregatedDirectoryExtensions
{
  public static bool IsAllFilesProcessed(this SegregatedDirectory segregationDirectory)
  {
    if (segregationDirectory is null || segregationDirectory.Files is null)
    {
      return false;
    }
    return segregationDirectory.Files.All(x => x.Status == FileStatus.Processed);
  }

  public static bool HasAnyFilesProcessed(this SegregatedDirectory segregationDirectory)
  {
    if (segregationDirectory is null || segregationDirectory.Files is null)
    {
      return false;
    }
    return segregationDirectory.Files.Any(x => x.Status == FileStatus.Processed);
  }

  public static bool IsAllFilesProcessed(this SegregatedDirectory[] segregationDirectory)
  {
    return segregationDirectory.All(x => x.IsAllFilesProcessed());
  }

  public static bool HasAnyFilesProcessed(this SegregatedDirectory[] segregationDirectory)
  {
    return segregationDirectory.All(x => x.HasAnyFilesProcessed());
  }
}
