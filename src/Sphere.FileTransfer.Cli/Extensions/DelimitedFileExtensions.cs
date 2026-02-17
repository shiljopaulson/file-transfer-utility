using Sphere.FileTransfer.Services.Models;

namespace Sphere.FileTransfer.Cli.Extensions;

public static class DelimitedExtensions
{

  public static bool IsAllLinesProcessed(this DelimitedFile delimitedFile)
  {
    if (delimitedFile.Lines is null || delimitedFile.Lines.Length == 0)
    {
      return false;
    }
    return delimitedFile.Lines.All(x => x.Status == LineStatus.Processed || x.Status == LineStatus.Skipped);
  }

  public static bool HasAnyLinesProcessed(this DelimitedFile delimitedFile)
  {
    if (delimitedFile.Lines is null || delimitedFile.Lines.Length == 0)
    {
      return false;
    }
    return delimitedFile.Lines.Any(x =>
      x.Status == LineStatus.Processed
      || x.Status == LineStatus.Skipped);
  }
}