using System.Collections.Immutable;
using System.Diagnostics;
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
        Trace.TraceInformation($"Not processing line number {i} due to '{Result.Lines[i].Status}({Result.Lines[i].Error})'");
        continue;
      }
      var fileName = Result.Lines[i].FileName;

      // Just to remove code warning
      if (string.IsNullOrWhiteSpace(fileName))
      {
        Result.Lines[i].Status = Status.Error;
        Result.Lines[i].Error = "Column Value Missing";
        Trace.TraceInformation($"Not processing line number {i} due to '{Result.Lines[i].Error}'");
        continue;
      }
      for (var j = 0; j < ParsedOptions.Sources.Length; j++)
      {
        var status = Result.Lines[i].Status;
        if (status == Status.Copied || status == Status.Moved)
        {
          j = ParsedOptions.Sources.Length;
          continue;
        }
        var sourceFilePath = Path.Combine(ParsedOptions.Sources[j].FullName, fileName);
        var destinationFilePath = Path.Combine(ParsedOptions.Destination.FullName, fileName);
        Trace.TraceInformation($"Initiating {ParsedOptions.Mode} for line number {i} ({sourceFilePath})");
        var result = CopyOrMove(sourceFilePath, destinationFilePath, cancellationToken);
        Result.Lines[i].Status = result.Item1;
        Result.Lines[i].Error = result.Item2;
      }
    }
  }
}
