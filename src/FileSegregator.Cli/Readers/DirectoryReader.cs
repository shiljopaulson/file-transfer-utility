using System.Security;
using FileSegregator.Cli.Models;

namespace FileSegregator.Cli.Readers;

internal sealed class DirectoryReader
{
  public SegregationDirectory GetFiles(DirectoryInfo directory, string searchPattern, CancellationToken cancellationToken)
  {
    cancellationToken.ThrowIfCancellationRequested();
    var segregationDirectory = new SegregationDirectory { Directory = directory };
    if (directory is null || !directory.Exists)
    {
      segregationDirectory.Status = Status.DirectoryNotFound;
      segregationDirectory.Message = $"Directory `{directory?.FullName}` not found.";
      return segregationDirectory;
    }
    var fileInfos = new List<FileInfo>();
    try
    {
      var files = directory.GetFiles(searchPattern);
      if (files is not null && files.Length > 0)
      {
        fileInfos.AddRange(files);
      }
    }
    catch (OperationCanceledException operationCanceledException)
    {
      segregationDirectory.Status = Status.OperationCanceled;
      segregationDirectory.Message = operationCanceledException.Message;
    }
    catch (ArgumentNullException argumentNullException)
    {
      segregationDirectory.Status = Status.Error;
      segregationDirectory.Message = argumentNullException.Message;
    }
    catch (ArgumentException argumentException)
    {
      segregationDirectory.Status = Status.Error;
      segregationDirectory.Message = argumentException.Message;
    }
    catch (DirectoryNotFoundException directoryNotFoundException)
    {
      segregationDirectory.Status = Status.Error;
      segregationDirectory.Message = directoryNotFoundException.Message;
    }
    catch (SecurityException securityException)
    {
      segregationDirectory.Status = Status.Error;
      segregationDirectory.Message = securityException.Message;
    }
    catch (Exception exception)
    {
      segregationDirectory.Status = Status.Error;
      segregationDirectory.Message = exception.Message;
    }
    if (fileInfos.Count == 0)
    {
      segregationDirectory.Status = Status.NoMatchingFilesFound;
      segregationDirectory.Message = $"No matching files found for '{directory.FullName}'";
    }
    segregationDirectory.Files = [.. fileInfos.Select(x => new SegregationFile { File = x })];
    return segregationDirectory;
  }
}