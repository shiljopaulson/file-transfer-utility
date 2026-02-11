using FileSegregator.Cli.Models;

namespace FileSegregator.Cli.Extensions;

public static class SegregationDirectoryExtensions
{
  public static bool IsAllFilesProcessed(this SegregationDirectory segregationDirectory)
  {
    return segregationDirectory.Files.All(x =>
      x.Status == Status.Copied
      || x.Status == Status.Moved
      || x.Status == Status.Skipped);
  }

  public static bool HasAnyFilesProcessed(this SegregationDirectory segregationDirectory)
  {
    return segregationDirectory.Files.Any(x =>
      x.Status == Status.Moved
      || x.Status == Status.Copied);
  }

  public static bool IsAllFilesProcessed(this SegregationDirectory[] segregationDirectory)
  {
    return segregationDirectory.All(x => x.IsAllFilesProcessed());
  }

  public static bool HasAnyFilesProcessed(this SegregationDirectory[] segregationDirectory)
  {
    return segregationDirectory.All(x => x.HasAnyFilesProcessed());
  }
}
