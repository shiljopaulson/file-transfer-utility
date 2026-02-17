using FluentValidation;
using Sphere.FileTransfer.Models;
using Sphere.FileTransfer.Services.Models;
using Sphere.FileTransfer.Services.Readers;

namespace Sphere.FileTransfer.Services;

public interface IPatternService
{
  Task<SegregatedDirectory[]> Process(PatternOptions patternOptions, CancellationToken cancellationToken);
}

public class PatternService : BaseFileService, IPatternService
{
  private readonly IDirectoryReader _directoryFileReader;
  private readonly AbstractValidator<PatternOptions> _patternOptionsValidator;

  public PatternService(IDirectoryReader directoryFileReader, AbstractValidator<PatternOptions> patternOptionsValidator)
  {
    _directoryFileReader = directoryFileReader;
    _patternOptionsValidator = patternOptionsValidator;
  }

  public async Task<SegregatedDirectory[]> Process(PatternOptions patternOptions, CancellationToken cancellationToken)
  {
    Console.WriteLine("Services.Process");
    cancellationToken.ThrowIfCancellationRequested();

    ThrowOnAnyValidationFails(patternOptions);
    var sources = patternOptions.Sources;
    var destination = patternOptions.Destination;
    var segregatedDirectories = _directoryFileReader.Read(sources, patternOptions.SearchPattern);

    var uniqueSourceFileNames = new Dictionary<string, SegregatedFile[]>();
    var uniqueDestinationFileNames = new Dictionary<string, string>();
    for (var i = 0; i < segregatedDirectories.Length; i++)
    {
      if (segregatedDirectories[i] is null
          || segregatedDirectories[i].Status != DirectoryStatus.Unprocessed
          || segregatedDirectories[i].Files is null
          || segregatedDirectories[i].Files.Length != 0)
      {
        continue;
      }
      for (var j = 0; j < segregatedDirectories[i].Files.Length; j++)
      {
        var fileName = segregatedDirectories[i].Files[j].File.Name;
        if (uniqueSourceFileNames.TryGetValue(fileName, out var segregatedFiles))
        {
          segregatedDirectories[i].Files[j].Status = FileStatus.Duplicate;
          segregatedDirectories[i].Files[j].Message = $"Originally found at '{uniqueSourceFileNames[fileName].First().File.FullName}'";
          uniqueSourceFileNames[fileName] = [.. uniqueSourceFileNames[fileName], segregatedDirectories[i].Files[j]];
          continue;
        }
        if (!uniqueDestinationFileNames.TryGetValue(fileName, out var _))
        {
          var destinationPath = Path.Combine(destination.FullName, fileName);
          uniqueDestinationFileNames[fileName] = File.Exists(destinationPath)
            ? $"File exist at '{destinationPath}'"
            : string.Empty;
        }
        uniqueSourceFileNames[fileName] = [segregatedDirectories[i].Files[j]];
      }
    }
    for (var i = 0; i < uniqueSourceFileNames.Count; i++)
    {
    }
    return [];
  }

  private static void ThrowOnAnyValidationFails(PatternOptions patternOptions)
  {
    ArgumentNullException.ThrowIfNull(patternOptions);


    if (string.IsNullOrWhiteSpace(patternOptions.SearchPattern))
    {
      throw new ArgumentException($"{nameof(patternOptions.SearchPattern)} not provided.");
    }
  }
}
