using System.Collections.Immutable;
using System.Diagnostics;
using FileSegregator.Cli.Constants;
using FileSegregator.Cli.Mappers;
using FileSegregator.Cli.Models;
using FileSegregator.Cli.Readers;

namespace FileSegregator.Cli.Services;

public sealed class DelimitedFileService : BaseFileService<DelimitedFileOptions, DelimitedFile>
{
  public static readonly ImmutableArray<Status> _fileStatusesToIgnore = [Status.Duplicate, Status.Error, Status.Skipped];

  private readonly IDelimitedFileReader _delimitedFileReader;
  public DelimitedFileService(DelimitedFileOptions options, IDelimitedFileReader delimitedFileReader, CancellationToken cancellationToken) : base(options)
  {
    ArgumentNullException.ThrowIfNull(delimitedFileReader);
    _delimitedFileReader = delimitedFileReader;
  }

  public override void Process(CancellationToken cancellationToken)
  {
    cancellationToken.ThrowIfCancellationRequested();
    if (ParsedOptions is null
      || ParsedOptions.Sources is null
      || ParsedOptions.Sources.Length == 0
      || ParsedOptions.Destination is null
      || ParsedOptions.InputFile is null)
    {
      Trace.TraceInformation("Guard check failed while initiating Delimited file Process");
      return;
    }

    var fieldIndex = ParsedOptions.Column;
    var skipHeader = !ParsedOptions.NoHeader;

    Result = _delimitedFileReader.Read(
      ParsedOptions.InputFile.FullName,
      EnumMappers.Map(ParsedOptions.Delimiter),
      skipHeader,
      --fieldIndex,
      cancellationToken);

    if (Result is null
      || Result.Lines is null
      || Result.Lines.Any(x => x.Status == Status.Failure))
    {
      Trace.TraceInformation("Delimited file reading is either empty or one of the line's failed.");
      return;
    }
    else if (Result.Status == Status.Failure
      || Result.Status == Status.OperationCanceled)
    {
      Trace.TraceInformation($"Delimited file reading stopped due to '{Result.Status}'.");
      return;
    }
    Trace.TraceInformation("Initiating Copy/Move Delimited file's all lines");
    for (int i = 0; i < Result.Lines.Length; i++)
    {
      cancellationToken.ThrowIfCancellationRequested();
      if (_fileStatusesToIgnore.Contains(Result.Lines[i].Status))
      {
        Trace.TraceInformation($"Not processing line number {i} due to '{Result.Lines[i].Status}({Result.Lines[i].Message})'");
        continue;
      }
      var fileName = Result.Lines[i].ColumnValue;

      // Just to remove code warning
      if (string.IsNullOrWhiteSpace(fileName))
      {
        Result.Lines[i].Status = Status.Error;
        Result.Lines[i].Message = "Column Value Missing";
        Trace.TraceInformation($"Not processing line number {i} due to '{Result.Lines[i].Message}'");
        continue;
      }
      //Console.WriteLine($"\nFileName:{fileName}");
      var uniqueFiles = new Dictionary<string, string>();
      var fileLog = new List<FileEntry>();
      for (var j = 0; j < ParsedOptions.Sources.Length; j++)
      {
        var sourceFilePath = Path.Combine(ParsedOptions.Sources[j].FullName, fileName);
        var destinationFilePath = Path.Combine(ParsedOptions.Destination.FullName, fileName);

        if (!uniqueFiles.TryGetValue(fileName, out var _))
        {
          uniqueFiles[fileName] = sourceFilePath;
        }

        if (ParsedOptions.DryRun)
        {
          var (sourceFileStatus, sourceFileMessage) = FileExist(sourceFilePath, cancellationToken);
          fileLog.Add(FileEntry.New(fileName, sourceFileStatus, sourceFileMessage));

          var (destinationFileStatus, destinationFileMessage) = FileExist(destinationFilePath, cancellationToken);
          if (destinationFileStatus == Status.FileFound)
          {
            fileLog.Add(FileEntry.New(fileName, Status.IO, destinationFileMessage));
          }

          if (fileLog.Count > 0 && j == (ParsedOptions.Sources.Length - 1))
          {
            var foundFileAtSource = fileLog.Exists(x => x.Status == Status.FileFound);
            var noFoundFileAtAnySource = fileLog.Exists(x => x.Status == Status.FileNotFound);
            if (!foundFileAtSource && noFoundFileAtAnySource)
            {
              Result.Lines[i].Status = Status.IO;
              Result.Lines[i].Message = $"File not found at any of the '{OptionNames.Sources}' locations.";
            }
            else if (ParsedOptions.Overwrite)
            {
              var entry = fileLog.Last(x => x.Status == Status.FileFound);
              Result.Lines[i].Status = entry.Status;
              Result.Lines[i].Message = entry.Message;
            }
            else
            {
              var entry = fileLog.First(x => x.Status == Status.FileFound);
              Result.Lines[i].Status = entry.Status;
              Result.Lines[i].Message = entry.Message;
            }

            if (ParsedOptions.Overwrite)
            {
              continue;
            }
            var destinationHasFile = fileLog.Exists(x => x.Status == Status.IO);
            if (destinationHasFile)
            {
              Result.Lines[i].Status = Status.IO;
              Result.Lines[i].Message = $"{Result.Lines[i].Message}File all ready exist at {OptionNames.Destination}('{destinationFilePath}')";
            }
          }

          continue;
        }

        Trace.TraceInformation($"Initiating {ParsedOptions.Operation} for line number {i} ({sourceFilePath})");
        var result = CopyOrMove(sourceFilePath, destinationFilePath, cancellationToken);
        fileLog.Add(FileEntry.New(fileName, result.Item1, result.Item2));

        var isCurrentSuccessful = SuccessfulStatuses.Contains<Status>(result.Item1);
        var hasCopiedOrMoved = fileLog.Exists(
          x => x.ColumnValue == fileName
          && SuccessfulStatuses.Contains<Status>(x.Status));

        if (isCurrentSuccessful)
        {
          Result.Lines[i].Status = result.Item1;
          Result.Lines[i].Message = result.Item2;
        }
        else if (hasCopiedOrMoved)
        {
          var entry = fileLog.Last(
            x => x.ColumnValue == fileName
            && SuccessfulStatuses.Contains<Status>(x.Status));
          Result.Lines[i].Status = entry.Status;
          Result.Lines[i].Message = entry.Message;
        }
        else
        {
          Result.Lines[i].Status = result.Item1;
          Result.Lines[i].Message = result.Item2;
        }
      }
    }
  }
  private class FileEntry
  {
    public string? ColumnValue { get; set; }
    public Models.Status Status { get; set; } = Models.Status.Unprocessed;
    public string? Message { get; set; } = string.Empty;

    public static FileEntry New(string? columnValue, Status status, string? message)
    {
      return new FileEntry { ColumnValue = columnValue, Status = status, Message = message };
    }
  }
}
