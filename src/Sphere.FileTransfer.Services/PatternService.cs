using FluentValidation;
using Microsoft.Extensions.Logging;
using Sphere.FileTransfer.Models;
using Sphere.FileTransfer.Services.Models;
using Sphere.FileTransfer.Services.Readers;

namespace Sphere.FileTransfer.Services;

public interface IPatternService
{
  Task<SegregatedDirectory[]> Process(PatternOptions patternOptions, CancellationToken cancellationToken);
}

public class PatternService : BaseFileService<PatternService>, IPatternService
{
  private readonly IDirectoryReader _directoryFileReader;
  private readonly AbstractValidator<PatternOptions> _patternOptionsValidator;

  public PatternService(IDirectoryReader directoryFileReader, AbstractValidator<PatternOptions> patternOptionsValidator, ILogger<PatternService> logger) : base(logger)
  {
    _directoryFileReader = directoryFileReader;
    _patternOptionsValidator = patternOptionsValidator;
  }

  public async Task<SegregatedDirectory[]> Process(PatternOptions patternOptions, CancellationToken cancellationToken)
  {
    cancellationToken.ThrowIfCancellationRequested();

    var validationResult = _patternOptionsValidator.Validate(patternOptions);
    if (!validationResult.IsValid)
    {
      _logger.LogError("Validation failed at {0}", string.Join(",", validationResult.Errors));
      return [];
    }

    var sources = patternOptions.Sources;
    var destination = patternOptions.Destination;
    var segregatedDirectories = _directoryFileReader.Read(sources, patternOptions.SearchPattern, cancellationToken);
    var overwrite = patternOptions.Overwrite;
    var dryRun = patternOptions.DryRun;
    var operation = patternOptions.Operation;

    var uniqueSourceFileNames = new Dictionary<string, SegregatedFile[]>();
    var uniqueDestinationFileNames = new Dictionary<string, string>();
    for (var i = 0; i < segregatedDirectories.Length; i++)
    {
      if (segregatedDirectories[i] is null
          || segregatedDirectories[i].Status != DirectoryStatus.Unprocessed
          || segregatedDirectories[i].Files is null
          || segregatedDirectories[i].Files?.Length == 0)
      {
        continue;
      }
      for (var j = 0; j < segregatedDirectories[i].Files?.Length; j++)
      {
        var fileName = segregatedDirectories[i].Files[j].File.Name;
        if (uniqueSourceFileNames.TryGetValue(fileName, out var segregatedFiles))
        {
          segregatedDirectories[i].Files[j].Status = FileStatus.Duplicate;
          segregatedDirectories[i].Files[j].Message = $"Originally found at '{uniqueSourceFileNames[fileName].First().File.FullName}'";
          uniqueSourceFileNames[fileName] = [.. segregatedFiles, segregatedDirectories[i].Files[j]];
          continue;
        }
        if (!uniqueDestinationFileNames.TryGetValue(fileName, out var _))
        {
          var destinationPath = Path.Combine(destination.FullName, fileName);
          var fileExistAtDestination = File.Exists(destinationPath);
          uniqueDestinationFileNames[fileName] = fileExistAtDestination
            ? $"File exist at '{destinationPath}'"
            : string.Empty;
          if (!overwrite && fileExistAtDestination)
          {
            segregatedDirectories[i].Files[j].Message = uniqueDestinationFileNames[fileName];
            segregatedDirectories[i].Files[j].Status = FileStatus.Error;
          }
        }
        uniqueSourceFileNames[fileName] = [segregatedDirectories[i].Files[j]];
      }
    }
    if (dryRun)
    {
      return segregatedDirectories;
    }
    FileStatus[] yetToProcessFileStatuses = [FileStatus.Unprocessed, FileStatus.Duplicate];
    for (var i = 0; i < uniqueSourceFileNames.Count; i++)
    {
      var fileName = uniqueSourceFileNames.Keys.ElementAt(i);
      var segregatedFiles = uniqueSourceFileNames[fileName];
      if (!overwrite && uniqueDestinationFileNames.TryGetValue(fileName, out var destinationFile) && !string.IsNullOrWhiteSpace(destinationFile))
      {
        continue;
      }
      if (overwrite)
      {
        for (var j = segregatedFiles.Length - 1; j > -1; j--)
        {
          if (!yetToProcessFileStatuses.Contains(segregatedFiles[j].Status))
          {
            continue;
          }
          var sourceFileFullName = segregatedFiles[j].File.FullName;
          var destinationFileFullName = Path.Combine(destination.FullName, segregatedFiles[j].File.Name);
          var (fileStatus, fileMessage) = CopyOrMove(sourceFileFullName, destinationFileFullName, operation, overwrite, cancellationToken);
          var filteredDirectory = GetDirectory(segregatedDirectories, segregatedFiles[j]);
          var filteredFile = GetSegregatedFile(filteredDirectory, sourceFileFullName);
          Update(ref filteredFile, fileStatus, fileMessage);

          if (fileStatus == FileStatus.Processed)
          {
            j = -1;
          }
        }
      }
      else
      {
        for (var j = 0; j < segregatedFiles.Length; j++)
        {
          if (!yetToProcessFileStatuses.Contains(segregatedFiles[j].Status))
          {
            continue;
          }
          var sourceFileFullName = segregatedFiles[j].File.FullName;
          var destinationFileFullName = Path.Combine(destination.FullName, segregatedFiles[j].File.Name);
          var (fileStatus, fileMessage) = CopyOrMove(sourceFileFullName, destinationFileFullName, operation, overwrite, cancellationToken);
          var filteredDirectory = GetDirectory(segregatedDirectories, segregatedFiles[j]);
          var filteredFile = GetSegregatedFile(filteredDirectory, sourceFileFullName);
          Update(ref filteredFile, fileStatus, fileMessage);
          if (fileStatus == FileStatus.Processed)
          {
            j = segregatedFiles.Length;
          }
        }
      }
    }
    return segregatedDirectories;
  }

  private SegregatedDirectory GetDirectory(SegregatedDirectory[] segregatedDirectories, SegregatedFile segregatedFile)
  {
    return segregatedDirectories.First(x => x.DirectoryPath == segregatedFile.File.FullName);
  }

  private SegregatedFile? GetSegregatedFile(SegregatedDirectory segregatedDirectory, string fileFullName)
  {
    return segregatedDirectory?.Files?.FirstOrDefault(x => x.File.FullName == fileFullName);
  }

  private void Update(ref SegregatedFile? segregatedFile, FileStatus fileStatus, string message)
  {
    if (segregatedFile is not null)
    {
      segregatedFile.Status = fileStatus;
      segregatedFile.Message = message;
    }
  }
}
