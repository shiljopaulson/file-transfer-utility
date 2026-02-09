using System.Security;
using FileSegregator.Cli.Models;

namespace FileSegregator.Cli.Readers;

internal sealed class DirectoryReader
{
  public SegregationDirectory GetFiles(DirectoryInfo[] directories, string searchPattern, CancellationToken cancellationToken)
  {
    cancellationToken.ThrowIfCancellationRequested();
    if (directories is null || directories.Length == 0)
    {
      return new SegregationDirectory { DirectoryNames = [] };
    }
    var segregationDirectory = new SegregationDirectory { DirectoryNames = [.. directories.Select(x => x.FullName)] };
    try
    {
      for (var i = 0; i < directories.Length; i++)
      {
        var files = directories[i].GetFiles(searchPattern);
        if (files is null || files.Length == 0)
        {
          segregationDirectory.Status = Status.NoMatchingFilesFound;
          segregationDirectory.Error = $"No matching files found for the search pattern '{searchPattern}'";
          return segregationDirectory;
        }
        segregationDirectory.Files = [.. segregationDirectory.Files, .. files.Select(x => new SegregationFile { FileName = x.Name, Status = Status.Unprocessed })];
      }
    }
    catch (OperationCanceledException operationCanceledException)
    {
      segregationDirectory.Status = Status.OperationCanceled;
      segregationDirectory.Error = operationCanceledException.Message;
    }
    catch (ArgumentNullException argumentNullException)
    {
      segregationDirectory.Status = Status.Error;
      segregationDirectory.Error = argumentNullException.Message;
    }
    catch (ArgumentException argumentException)
    {
      segregationDirectory.Status = Status.Error;
      segregationDirectory.Error = argumentException.Message;
    }
    catch (DirectoryNotFoundException directoryNotFoundException)
    {
      segregationDirectory.Status = Status.Error;
      segregationDirectory.Error = directoryNotFoundException.Message;
    }
    catch (SecurityException securityException)
    {
      segregationDirectory.Status = Status.Error;
      segregationDirectory.Error = securityException.Message;
    }
    catch (Exception exception)
    {
      segregationDirectory.Status = Status.Error;
      segregationDirectory.Error = exception.Message;
    }
    return segregationDirectory;
  }
}