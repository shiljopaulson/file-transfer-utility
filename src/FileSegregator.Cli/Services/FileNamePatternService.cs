using System.Diagnostics;
using FileSegregator.Cli.Models;
using FileSegregator.Cli.Readers;

namespace FileSegregator.Cli.Services;

public sealed class FileNamePatternService(FileNamePatternOption parsedOptions) : BaseFileService<FileNamePatternOption, SegregationDirectory>(parsedOptions)
{
  public override void Process(CancellationToken cancellationToken)
  {
    cancellationToken.ThrowIfCancellationRequested();
    if (ParsedOptions is null
      || ParsedOptions.Source is null
      || ParsedOptions.Destination is null
      || string.IsNullOrWhiteSpace(ParsedOptions.FileNamePattern))
    {
      Trace.TraceInformation("Guard check failed while initiating File name pattern Process");
      return;
    }

    var directoryReader = new DirectoryReader();
    Result = directoryReader.GetFiles(ParsedOptions.Destination, ParsedOptions.FileNamePattern, cancellationToken);
    if (Result is null
      || Result.Files is null
      || Result.Files.Length == 0)
    {
      Trace.TraceInformation($"Directory doesn't have files due to '{Result?.Error}'");
      return;
    }

    for (var i = 0; i < Result.Files.Length; i++)
    {
      var fileName = Result.Files[i].FileName;
      var sourceFilePath = Path.Combine(ParsedOptions.Source.FullName, fileName);
      var destinationFilePath = Path.Combine(ParsedOptions.Destination.FullName, fileName);
      Trace.TraceInformation($"Initiating {ParsedOptions.Mode} for file ({sourceFilePath})");
      var result = CopyOrMove(sourceFilePath, destinationFilePath, cancellationToken);
      Result.Files[i].Status = result.Item1;
      Result.Files[i].Error = result.Item2;
    }
  }
}
