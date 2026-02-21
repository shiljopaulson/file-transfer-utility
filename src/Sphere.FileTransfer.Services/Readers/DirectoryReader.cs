using Microsoft.Extensions.Logging;
using Sphere.FileTransfer.Services.Models;

namespace Sphere.FileTransfer.Services.Readers;

public interface IDirectoryReader
{
  SegregatedDirectory[] Read(DirectoryInfo[] directories, string searchPattern, CancellationToken cancellationToken);
}

public sealed class DirectoryReader : IDirectoryReader
{
  private readonly ILogger<DirectoryReader> _logger;
  public DirectoryReader(ILogger<DirectoryReader> logger)
  {
    _logger = logger;
  }
  private SegregatedDirectory Read(DirectoryInfo directoryInfo, string searchPattern, CancellationToken cancellationToken)
  {
    _logger.LogTrace("Entering IDirectoryReader => Read");
    var segregatedDirectory = new SegregatedDirectory
    {
      DirectoryPath = directoryInfo.FullName
    };
    try
    {
      cancellationToken.ThrowIfCancellationRequested();
      var files = directoryInfo.GetFiles(searchPattern);
      if (files is null || files.Length == 0)
      {
        segregatedDirectory.Status = DirectoryStatus.NoMatchingFiles;
        return segregatedDirectory;
      }
      segregatedDirectory.Files = files.Select(x => new SegregatedFile { File = x }).ToArray();
    }
    catch (OperationCanceledException exception)
    {
      _logger.LogError(exception.Message);
      segregatedDirectory.Status = DirectoryStatus.Canceled;
    }
    catch (Exception exception)
    {
      _logger.LogError(exception.Message);
      segregatedDirectory.Status = DirectoryStatus.Error;
    }
    return segregatedDirectory;
  }

  public SegregatedDirectory[] Read(DirectoryInfo[] directories, string searchPattern, CancellationToken cancellationToken)
  {
    return [.. directories.Select(x => Read(x, searchPattern, cancellationToken))];
  }
}
