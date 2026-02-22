using FluentValidation;

using Microsoft.Extensions.Logging;

using Sphere.FileTransfer.Models;
using Sphere.FileTransfer.Services.Models;
using Sphere.FileTransfer.Services.Readers;

namespace Sphere.FileTransfer.Services;

public interface IDelimitedService
{
  Task<DelimitedFile?> Process(DelimitedOptions delimitedOptions, CancellationToken cancellationToken);
}

public sealed class DelimitedService : BaseFileService<DelimitedService>, IDelimitedService
{
  private readonly IDelimitedReader _delimitedReader;
  private readonly AbstractValidator<DelimitedOptions> _delimitedOptionsValidator;

  public DelimitedService(IDelimitedReader delimitedReader, AbstractValidator<DelimitedOptions> delimitedOptionsValidator, ILogger<DelimitedService> logger) : base(logger)
  {
    _delimitedReader = delimitedReader;
    _delimitedOptionsValidator = delimitedOptionsValidator;
  }

  public async Task<DelimitedFile?> Process(DelimitedOptions delimitedOptions, CancellationToken cancellationToken)
  {
    Logger.LogTrace("Entering IDelimitedService => Process");
    cancellationToken.ThrowIfCancellationRequested();

    var validationResult = await _delimitedOptionsValidator.ValidateAsync(delimitedOptions, cancellationToken).ConfigureAwait(true);
    if (!validationResult.IsValid)
    {
      Logger.LogError("Validation failed at {Errors}", string.Join(",", validationResult.Errors));
      return new DelimitedFile { FileFullName = delimitedOptions.File.FullName, Status = FileStatus.Error };
    }
    var sources = delimitedOptions.Sources;
    var destination = delimitedOptions.Destination;
    var delimitedFile = delimitedOptions.File;
    var column = delimitedOptions.Column;
    var fieldIndex = column - 1;
    var overwrite = delimitedOptions.Overwrite;
    var operation = delimitedOptions.Operation;
    var dryRun = delimitedOptions.DryRun;
    var delimiter = delimitedOptions.Delimiter;

    var delimitedResult = _delimitedReader.Read(delimitedFile.FullName, delimiter, !delimitedOptions.NoHeader, cancellationToken);
    if (delimitedResult is null
      || delimitedResult.Lines.Length == 0)
    {
      return new DelimitedFile { FileFullName = delimitedOptions.File.FullName };
    }
    var uniqueFiles = new Dictionary<string, DetailedFileInfo[]>();
    for (var i = 0; i < delimitedResult?.Lines.Length; i++)
    {
      cancellationToken.ThrowIfCancellationRequested();
      if (delimitedResult is null
        || delimitedResult.Lines[i] is null)
      {
        continue;
      }

      var (status, message) = LineValidation(delimitedResult.Lines[i], fieldIndex);
      delimitedResult.Lines[i].Status = status;
      delimitedResult.Lines[i].Message = message;
      if (status != LineStatus.Unprocessed)
      {
        continue;
      }

      var fileName = delimitedResult.Lines[i].DelimitedFields[fieldIndex].Trim();
      if (string.IsNullOrWhiteSpace(fileName))
      {
        continue;
      }
      var destinationFilePath = Path.Combine(destination.FullName, fileName);
      if (uniqueFiles.TryGetValue(fileName, out var detailedFileInfos))
      {
        delimitedResult.Lines[i].Status = LineStatus.Duplicate;
        delimitedResult.Lines[i].Message = $"File entry initially found at '{detailedFileInfos.FirstOrDefault()?.LineNumber}'";
        continue;
      }
      else
      {
        uniqueFiles[fileName] = overwrite
          ? []
          : [DetailedFileInfo.GetStatusForDestination(destinationFilePath, i)];
      }

      for (var j = 0; j < sources.Length; j++)
      {
        cancellationToken.ThrowIfCancellationRequested();
        var sourceFilePath = Path.Combine(sources[j].FullName, fileName);

        var newEntry = DetailedFileInfo.GetStatusForSource(sourceFilePath, i);
        var hasEntries = uniqueFiles[fileName].Length > 0;

        uniqueFiles[fileName] = hasEntries
          ? [.. uniqueFiles[fileName], newEntry]
          : [newEntry];
      }

      if (uniqueFiles.TryGetValue(fileName, out var fileInfos))
      {
        var statuses = fileInfos
          .SelectMany(x => x.Statuses)
          .ToArray();

        var (destinationFileExist, destinationFileExistMessage)
          = TryGetDestinationFileExist(statuses, overwrite);

        var (isSourceFileNotFound, isSourceFileNotFoundMessage)
          = TryGetSourceFileNotFound(statuses, overwrite);

        var sourceFound = overwrite
          ? statuses.LastOrDefault(x => x.Status == Status.SourceFileFound)
          : statuses.FirstOrDefault(x => x.Status == Status.SourceFileFound);

        if (sourceFound is not null)
        {
          if (dryRun)
          {
            delimitedResult?.Lines[i].Status = LineStatus.Unprocessed;
            delimitedResult?.Lines[i].Message = sourceFound.Message;
          }
          else
          {
            var detailedFileStatus = overwrite
              ? fileInfos.SelectMany(x => x.Statuses).LastOrDefault(x => x.Status == Status.SourceFileFound)
              : fileInfos.SelectMany(x => x.Statuses).FirstOrDefault(x => x.Status == Status.SourceFileFound);
            if (detailedFileStatus is null)
            {
              continue;
            }
            var (fileStatus, fileMessage) = CopyOrMove(detailedFileStatus.FilePath, destinationFilePath, operation, overwrite, cancellationToken);
            delimitedResult?.Lines[i].Status = Mappers.Map(fileStatus);
            delimitedResult?.Lines[i].Message = fileMessage;
          }
        }

        var errorList = new List<string?>();
        if (destinationFileExist)
        {
          errorList.Add(destinationFileExistMessage);
        }
        if (sourceFound is null && isSourceFileNotFound)
        {
          errorList.Add(isSourceFileNotFoundMessage);
        }
        if (errorList.Count > 0)
        {
          delimitedResult?.Lines[i].Status = LineStatus.Error;
          delimitedResult?.Lines[i].Message = string.Join(", ", errorList);
        }
      }
    }
    return delimitedResult;
  }

  private static (LineStatus, string) LineValidation(DelimitedFileLine? delimitedFileLine, int fieldIndex)
  {
    if (delimitedFileLine is null)
    {
      return (LineStatus.Error, "Empty line.");
    }
    else if (delimitedFileLine.Status == LineStatus.Skipped)
    {
      return (LineStatus.Skipped, string.Empty);
    }
    else if (delimitedFileLine.DelimitedFields.Length == 0)
    {
      return (LineStatus.Error, "Empty line.");
    }
    else if (delimitedFileLine.DelimitedFields.Length < fieldIndex)
    {
      return (LineStatus.Error, $"Has less number of columns than expected column number '{fieldIndex + 1}'.");
    }
    else if (string.IsNullOrWhiteSpace(delimitedFileLine.DelimitedFields[fieldIndex].Trim()))
    {
      return (LineStatus.Error, "File name missing.");
    }
    return (LineStatus.Unprocessed, string.Empty);
  }

  private static (bool, string?) TryGetDestinationFileExist(DetailedFileStatus[] statuses, bool overwrite)
  {
    var destinationFile = overwrite
          ? statuses.LastOrDefault(x => x.Status == Status.DestinationFileExist)
          : statuses.FirstOrDefault(x => x.Status == Status.DestinationFileExist);

    if (destinationFile is not null)
    {
      return (true, destinationFile.Message);
    }
    return (false, string.Empty);
  }

  private static (bool, string?) TryGetSourceFileNotFound(DetailedFileStatus[] statuses, bool overwrite)
  {
    var sourceNotFound = overwrite
          ? statuses.LastOrDefault(x => x.Status == Status.SourceFileNotFound)
          : statuses.FirstOrDefault(x => x.Status == Status.SourceFileNotFound);

    if (sourceNotFound is not null)
    {
      return (true, sourceNotFound.Message);
    }
    return (false, string.Empty);
  }

  private sealed class DetailedFileInfo
  {
    public required FileInfo File { get; set; }
    public required int LineNumber { get; set; }
    public DetailedFileStatus[] Statuses { get; set; } = [];

    public static DetailedFileInfo GetStatusForSource(string fileFullName, int lineNumber)
    {
      FileInfo file = new(fileFullName);
      var fileInfo = new DetailedFileInfo { File = file, LineNumber = lineNumber };
      if (!fileInfo.File.Exists)
      {
        var detailedFileStatus = new DetailedFileStatus
        {
          FilePath = fileFullName,
          Status = Status.SourceFileNotFound,
          Message = $"Cannot find file at '{fileInfo.File.FullName}'"
        };
        fileInfo.Statuses = [detailedFileStatus];
      }
      else
      {
        var detailedFileStatus = new DetailedFileStatus
        {
          FilePath = fileFullName,
          Status = Status.SourceFileFound,
          Message = $"Found file at '{fileInfo.File.FullName}'"
        };
        fileInfo.Statuses = [detailedFileStatus];
      }
      return fileInfo;
    }

    public static DetailedFileInfo GetStatusForDestination(string fileFullName, int lineNumber)
    {
      FileInfo file = new(fileFullName);
      var fileErrors = new DetailedFileInfo { File = file, LineNumber = lineNumber };
      if (fileErrors.File.Exists)
      {
        var detailedFileError = new DetailedFileStatus
        {
          FilePath = fileFullName,
          Status = Status.DestinationFileExist,
          Message = $"File already exist at '{fileErrors.File.FullName}'"
        };
        fileErrors.Statuses = [detailedFileError];
      }
      return fileErrors;
    }
  }

  private sealed class DetailedFileStatus
  {
    public required string FilePath { get; set; }
    public Status? Status { get; set; }
    public string? Message { get; set; }
  }

  private enum Status
  {
    SourceFileFound,
    SourceFileNotFound,
    DestinationFileExist
  }
}