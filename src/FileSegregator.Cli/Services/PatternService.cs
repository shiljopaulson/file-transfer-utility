using System.Diagnostics;
using FileSegregator.Cli.Models;
using FileSegregator.Cli.Readers;

namespace FileSegregator.Cli.Services;

public sealed class PatternService(PatternOptions parsedOptions) : BaseFileService<PatternOptions, SegregationDirectory>(parsedOptions)
{
  public override void Process(CancellationToken cancellationToken)
  {
    cancellationToken.ThrowIfCancellationRequested();
    if (ParsedOptions is null
      || ParsedOptions.Sources is null
      || ParsedOptions.Destination is null
      || string.IsNullOrWhiteSpace(ParsedOptions.SearchPattern))
    {
      Trace.TraceInformation("Guard check failed while initiating File name pattern Process");
      return;
    }

    var directoryReader = new DirectoryReader();

    Result = directoryReader.GetFiles(ParsedOptions.Sources, ParsedOptions.SearchPattern, cancellationToken);
    if (Result is null
      || Result.Files is null
      || Result.Files.Length == 0)
    {
      Trace.TraceInformation($"Directory doesn't have files due to '{Result?.Error}'");
      return;
    }
    for (var i = 0; i < Result.Files.Length; i++)
    {
      for (var j = 0; j < ParsedOptions.Sources.Length; j++)
      {
        var status = Result.Files[i].Status;
        if (status == Status.Copied || status == Status.Moved)
        {
          j = ParsedOptions.Sources.Length;
          continue;
        }
        var fileName = Result.Files[i].FileName;
        var sourceFilePath = Path.Combine(ParsedOptions.Sources[j].FullName, fileName);
        var destinationFilePath = Path.Combine(ParsedOptions.Destination.FullName, fileName);
        Trace.TraceInformation($"Initiating {ParsedOptions.Mode} for file ({sourceFilePath})");
        var result = CopyOrMove(sourceFilePath, destinationFilePath, cancellationToken);
        Result.Files[i].Status = result.Item1;
        Result.Files[i].Error = result.Item2;
      }
    }
  }
}
