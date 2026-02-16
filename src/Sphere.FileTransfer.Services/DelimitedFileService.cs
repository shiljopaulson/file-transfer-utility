using System.Text;
using Sphere.FileTransfer.Models;
using Sphere.FileTransfer.Services.Models;
using Sphere.FileTransfer.Services.Readers;

namespace Sphere.FileTransfer.Services;

public interface IDelimitedFileService
{
  Task<DelimitedFile> Process(DelimitedFileOptions delimitedFileOptions, CancellationToken cancellationToken);
}

public class DelimitedFileService : BaseFileService, IDelimitedFileService
{
  private readonly IDelimitedFileReader _delimitedFileReader;

  public DelimitedFileService(IDelimitedFileReader delimitedFileReader)
  {
    ArgumentNullException.ThrowIfNull(delimitedFileReader);
    _delimitedFileReader = delimitedFileReader;
  }

  public async Task<DelimitedFile> Process(DelimitedFileOptions delimitedFileOptions, CancellationToken cancellationToken)
  {
    Console.WriteLine("Services.Process");
    cancellationToken.ThrowIfCancellationRequested();

    ThrowOnAnyValidationFails(delimitedFileOptions);
    var sources = delimitedFileOptions.Sources;
    var destination = delimitedFileOptions.Destination;
    var delimitedFile = delimitedFileOptions.DelimitedFile;
    var column = delimitedFileOptions.Column;
    var fieldIndex = column - 1;
    var overwrite = delimitedFileOptions.Overwrite;
    var operation = delimitedFileOptions.Operation;
    var dryRun = delimitedFileOptions.DryRun;
    var delimiter = delimitedFileOptions.Delimiter;

    var delimitedFileResult = _delimitedFileReader.Read(delimitedFile.FullName, delimiter, !delimitedFileOptions.NoHeader, cancellationToken);
    if (delimitedFileResult is null
      || delimitedFileResult.Lines is null
      || delimitedFileResult.Lines.Length == 0)
    {
      return delimitedFileResult;
    }
    var uniqueFiles = new Dictionary<string, DetailedFileInfo[]>();
    for (var i = 0; i < delimitedFileResult.Lines.Length; i++)
    {
      //Console.WriteLine($"{delimitedFileResult.Lines[i].Data}");
      cancellationToken.ThrowIfCancellationRequested();

      var (status, message) = LineValidation(delimitedFileResult.Lines[i], fieldIndex);
      delimitedFileResult.Lines[i].Status = status;
      delimitedFileResult.Lines[i].Message = message;
      if (status != LineStatus.Unprocessed)
      {
        continue;
      }

      var fileName = delimitedFileResult.Lines[i].DelimitedFields[fieldIndex].Trim();
      var destinationFilePath = Path.Combine(destination.FullName, fileName);
      if (uniqueFiles.TryGetValue(fileName, out var detailedFileInfos))
      {
        delimitedFileResult.Lines[i].Status = LineStatus.Duplicate;
        delimitedFileResult.Lines[i].Message = $"File entry initially found at '{detailedFileInfos.First().LineNumber}'";
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
        //Console.WriteLine($"hasEntries:{hasEntries} for fileName:{fileName}");

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
            delimitedFileResult.Lines[i].Status = LineStatus.Unprocessed;
            delimitedFileResult.Lines[i].Message = sourceFound.Message;
          }
          else
          {
            var detailedFileStatus = overwrite
              ? fileInfos.SelectMany(x => x.Statuses).LastOrDefault(x => x.Status == Status.SourceFileFound)
              : fileInfos.SelectMany(x => x.Statuses).FirstOrDefault(x => x.Status == Status.SourceFileFound);
            var (fileStatus, fileMessage) = CopyOrMove(detailedFileStatus.FilePath, destinationFilePath, operation, overwrite, cancellationToken);
            delimitedFileResult.Lines[i].Status = Mappers.Map(fileStatus);
            delimitedFileResult.Lines[i].Message = fileMessage;
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
          delimitedFileResult.Lines[i].Status = LineStatus.Error;
          delimitedFileResult.Lines[i].Message = string.Join(", ", errorList);
        }
      }
    }
    return delimitedFileResult;
  }

  private static void ThrowOnAnyValidationFails(DelimitedFileOptions delimitedFileOptions)
  {
    ArgumentNullException.ThrowIfNull(delimitedFileOptions);
    var sources = delimitedFileOptions.Sources;
    ArgumentNullException.ThrowIfNull(sources);
    if (sources.Length == 0)
    {
      throw new ArgumentException($"'{nameof(sources)}' missing");
    }
    if (!sources.All(x => x.Exists))
    {
      throw new DirectoryNotFoundException($"one or more '{nameof(sources)}' directory not found");
    }

    var destination = delimitedFileOptions.Destination;
    ArgumentNullException.ThrowIfNull(destination);
    if (!destination.Exists)
    {
      throw new DirectoryNotFoundException($"'{nameof(destination)}' directory not found");
    }
    if (sources.Any(x => x.FullName == destination.FullName))
    {
      throw new ArgumentException($"one or more '{nameof(sources)}' directory is same as {nameof(destination)}");
    }

    var delimitedFile = delimitedFileOptions.DelimitedFile;
    ArgumentNullException.ThrowIfNull(delimitedFile);
    if (!delimitedFile.Exists)
    {
      throw new FileNotFoundException(nameof(delimitedFile));
    }

    var column = delimitedFileOptions.Column;
    if (column < 1)
    {
      throw new ArgumentException($"'{nameof(column)}' cannot be less than 1");
    }
  }

  private static (LineStatus, string) LineValidation(DelimitedFileLine delimitedFileLine, int fieldIndex)
  {
    if (delimitedFileLine is null)
    {
      return (LineStatus.Error, "Empty line.");
    }
    else if (delimitedFileLine.Status == LineStatus.Skipped)
    {
      return (LineStatus.Skipped, string.Empty);
    }
    else if (delimitedFileLine.DelimitedFields is null
      || delimitedFileLine.DelimitedFields.Length == 0)
    {
      return (LineStatus.Error, "Empty line.");
    }
    else if (delimitedFileLine.DelimitedFields.Length < fieldIndex)
    {
      return (LineStatus.Error, $"Has less number of columns than expected column number '{fieldIndex + 1}'.");
    }
    if (string.IsNullOrWhiteSpace(delimitedFileLine.DelimitedFields[fieldIndex].Trim()))
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

  private class DetailedFileInfo
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

  private class DetailedFileStatus
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