using System.Collections.Immutable;
using System.Diagnostics;
using FileSegregator.Cli.Models;
using FileSegregator.Cli.Options;
using FileSegregator.Cli.Readers;

namespace FileSegregator.Cli.Services;

public sealed class PatternService(PatternOptions parsedOptions) : BaseFileService<PatternOptions, SegregationDirectory[]>(parsedOptions)
{
  public override void Process(CancellationToken cancellationToken)
  {
    cancellationToken.ThrowIfCancellationRequested();
    if (ParsedOptions is null
      || ParsedOptions.Sources is null
      || ParsedOptions.Destination is null
      || string.IsNullOrWhiteSpace(ParsedOptions.SearchPattern))
    {
      Trace.TraceInformation("Guard check failed while initiating Search pattern Process");
      return;
    }
    var directoryReader = new DirectoryReader();
    var segregationDirectories = new SegregationDirectory[2];
    for (var i = 0; i < ParsedOptions.Sources.Length; i++)
    {
      segregationDirectories[i] = directoryReader.GetFiles(ParsedOptions.Sources[i], ParsedOptions.SearchPattern, cancellationToken);
    }
    Result = segregationDirectories;

    var uniqueFiles = new Dictionary<string, string>();
    var fileLog = new List<FileEntry>();
    for (var i = 0; i < Result.Length; i++)
    {
      if (Result[i].Status != Status.Unprocessed)
      {
        Trace.TraceInformation($"Directory:'{Result[i].Directory?.FullName}', Status:'{Result[i].Status}', Message:{Result[i]?.Message}");
        continue;
      }
      for (var j = 0; j < Result[i].Files.Length; j++)
      {
        var fileName = Result[i].Files[j].File.FullName.Split(Path.DirectorySeparatorChar).Last();
        var sourceFilePath = Result[i].Files[j].File.FullName;
        var destinationFilePath = Path.Combine(ParsedOptions.Destination.FullName, fileName);

        if (uniqueFiles.TryGetValue(fileName, out var firstFoundDirectoryName))
        {
          var firstOrCurrentPath = ParsedOptions.Overwrite ? sourceFilePath : firstFoundDirectoryName;
          var operationName = ParsedOptions.Operation == Operation.Copy ? "copied" : "moved";
          Result[i].Files[j].Status = Status.Duplicate;
          Result[i].Files[j].Message = $"The file '{fileName}' will be {operationName} from '{firstOrCurrentPath}'.";
          fileLog.Add(FileEntry.New(fileName, Result[i].Files[j].Status, Result[i].Files[j].Message));
        }
        else
        {
          Result[i].Files[j].Status = Status.FileFound;
          uniqueFiles[fileName] = sourceFilePath;
        }

        if (!ParsedOptions.Overwrite)
        {
          var (destinationFileStatus, destinationFileMessage) = FileExist(destinationFilePath, cancellationToken);
          if (destinationFileStatus == Status.FileFound)
          {
            Result[i].Files[j].Status = Status.FileFoundAtDestination;
            Result[i].Files[j].Message = destinationFileMessage;
            fileLog.Add(FileEntry.New(fileName, Status.FileFoundAtDestination, destinationFileMessage));
          }
        }

        if (ParsedOptions.DryRun)
        {
          continue;
        }

        Trace.TraceInformation($"Initiating '{ParsedOptions.Operation}' for file '{sourceFilePath}'");
        var result = CopyOrMove(sourceFilePath, destinationFilePath, cancellationToken);
        fileLog.Add(FileEntry.New(fileName, result.Item1, result.Item2));

        var isCurrentSuccessful = SuccessfulStatuses.Contains<Status>(result.Item1);
        var hasCopiedOrMoved = fileLog.Exists(x => x.FileName == fileName && SuccessfulStatuses.Contains<Status>(x.Status));
        //var hasDuplicate = fileLog.Exists(x => x.FileName == fileName && x.Status == Status.Duplicate);
        if (isCurrentSuccessful)
        {
          Result[i].Files[j].Status = result.Item1;
          Result[i].Files[j].Message = result.Item2;
        }
        else if (hasCopiedOrMoved)
        {
          var entry = fileLog.Last(x => x.FileName == fileName && SuccessfulStatuses.Contains<Status>(x.Status));
          Result[i].Files[j].Status = entry.Status;
          Result[i].Files[j].Message = entry.Message;
        }
        else
        {
          Result[i].Files[j].Status = result.Item1;
          Result[i].Files[j].Message = result.Item2;
        }
      }
    }
  }

  private class FileEntry
  {
    public string? FileName { get; set; }
    public Models.Status Status { get; set; } = Models.Status.Unprocessed;
    public string? Message { get; set; } = string.Empty;

    public static FileEntry New(string? fileName, Status status, string? message)
    {
      return new FileEntry { FileName = fileName, Status = status, Message = message };
    }
  }
}
