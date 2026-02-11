using FileSegregator.Cli.Models;

namespace FileSegregator.Cli.Extensions;

public static class DelimitedFileExtensions
{

  public static bool IsAllLinesProcessed(this DelimitedFile delimitedFile)
  {
    if (delimitedFile.Lines is null || delimitedFile.Lines.Length == 0)
    {
      return false;
    }
    return delimitedFile.Lines.All(x => x.Status == Status.Copied || x.Status == Status.Moved || x.Status == Status.Skipped);
  }

  public static bool HasAnyLinesProcessed(this DelimitedFile delimitedFile)
  {
    if (delimitedFile.Lines is null || delimitedFile.Lines.Length == 0)
    {
      return false;
    }
    return delimitedFile.Lines.Any(x =>
      x.Status == Status.Moved
      || x.Status == Status.Copied);
  }
}