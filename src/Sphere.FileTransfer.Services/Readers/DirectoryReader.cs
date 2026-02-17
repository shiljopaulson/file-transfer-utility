using Sphere.FileTransfer.Services.Models;

namespace Sphere.FileTransfer.Services.Readers;

public interface IDirectoryReader
{
  SegregatedDirectory[] Read(DirectoryInfo[] directories, string searchPattern);
}

public sealed class DirectoryReader : IDirectoryReader
{
  private SegregatedDirectory Read(DirectoryInfo directoryInfo, string searchPattern)
  {
    var segregatedDirectory = new SegregatedDirectory
    {
      DirectoryPath = directoryInfo.FullName
    };
    var files = directoryInfo.GetFiles(searchPattern);
    if (files is null || files.Length == 0)
    {
      segregatedDirectory.Status = DirectoryStatus.NoMatchingFiles;
      return segregatedDirectory;
    }
    segregatedDirectory.Files = files.Select(x => new SegregatedFile { File = x }).ToArray();
    return segregatedDirectory;
  }

  public SegregatedDirectory[] Read(DirectoryInfo[] directories, string searchPattern)
  {
    return [.. directories.Select(x => Read(x, searchPattern))];
  }
}
