using System.CommandLine;
using System.Diagnostics;
using Sphere.FileTransfer.Cli.Constants;
using Sphere.FileTransfer.Cli.Extensions;
using Sphere.FileTransfer.Cli.Models;
using Sphere.FileTransfer.Cli.Options;
using Sphere.FileTransfer.Models;
using Sphere.FileTransfer.Services;
using Sphere.FileTransfer.Services.Models;

namespace Sphere.FileTransfer.Cli.Commands;

public sealed class DelimitedFileCommand : BaseCommand<DelimitedFileOptions, DelimitedFile>
{
  private static readonly string _description;
  private readonly IDelimitedFileService _delimitedFileService;

  static DelimitedFileCommand()
  {
    var delimiters = Enum.GetNames<Delimiter>().Select(x => x.ToLowerInvariant());
    _description = $"Copies or Movies files based on the entries found in the delimited file ({string.Join(",", delimiters)})";
  }

  public DelimitedFileCommand(IDelimitedFileService delimitedFileService, string name = "delimited") : base(name, "")
  {
    Description = _description;
    _delimitedFileService = delimitedFileService;
  }

  public override Command Create()
  {
    Options.Add(new SourcesOption());
    Options.Add(new DestinationOption());
    Options.Add(new DelimitedFileOption());
    Options.Add(new ColumnOption());
    Options.Add(new DelimiterOption());
    Options.Add(new OperationOption());
    Options.Add(new NoHeaderOption());
    Options.Add(new OutputFormatOption());
    Options.Add(new OverwriteOption());
    Options.Add(new DryRunOption());
    Options.Add(new QuietOption());

    SetAction(Invoke());
    return this;
  }

  private Func<ParseResult, CancellationToken, Task<int>> Invoke()
  {
    return async (parseResult, cancellationToken) =>
    {
      cancellationToken.ThrowIfCancellationRequested();
      if (parseResult.GetValue<bool>(OptionNames.Help))
      {
        Parse(OptionNames.Help).Invoke();
        return ExitCodes.Success;
      }
      ParsedOptions = Mappers.MapToDelimitedFileOptions(parseResult);
      return await Execute(parseResult, cancellationToken);
    };
  }

  internal override async Task<int> Process(CancellationToken cancellationToken)
  {
    if (ParsedOptions is null)
    {
      return ExitCodes.Error;
    }

    Result = await _delimitedFileService.Process(ParsedOptions, cancellationToken);

    if (Result is null)
    {
      return ExitCodes.Error;
    }
    if (Result.Status == FileStatus.Error
      || Result.Lines is null
      || Result.Lines.Length == 0)
    {
      return ExitCodes.Error;
    }
    if (ParsedOptions.DryRun)
    {
      Result.Status = FileStatus.Processed;
      return ExitCodes.Success;
    }
    else if (Result.IsAllLinesProcessed())
    {
      Result.Status = FileStatus.Processed;
      return ExitCodes.Success;
    }
    else if (Result.HasAnyLinesProcessed())
    {
      Result.Status = FileStatus.Processed;
      return ExitCodes.PartialSuccess;
    }
    else
    {
      Result.Status = FileStatus.Error;
      return ExitCodes.Error;
    }
  }

  internal override void Print(CancellationToken cancellationToken)
  {
    if (ParsedOptions is null
      || Result is null
      || Result.Lines is null)
    {
      return;
    }
    if (Result.Lines.Length == 0)
    {

    }
    Trace.TraceInformation($"Initiating print as '{OutputFormat}'");
    if (OutputFormat == OutputFormat.JSON)
    {
      ConsoleJson(cancellationToken);
    }
    else
    {
      ConsoleText(cancellationToken);
    }
  }

  private void ConsoleJson(CancellationToken cancellationToken)
  {
    Trace.TraceInformation("DelimitedFileCommand.ConsoleJson()");
    if (Result is null
      || Result.Lines is null
      || Result.Lines.Length == 0)
    {
      return;
    }
    Console.WriteLine(Utility.ToJson(Result));
  }

  private void ConsoleText(CancellationToken cancellationToken)
  {
    Trace.TraceInformation("DelimitedFileCommand.ConsoleText()");
    if (Result is null
      || Result.Lines is null
      || Result.Lines.Length == 0)
    {
      return;
    }
    foreach (var item in Result.Lines)
    {
      Trace.TraceInformation("DelimitedFileCommand.ConsoleText() - for");
      cancellationToken.ThrowIfCancellationRequested();
      switch (item.Status)
      {
        case LineStatus.Skipped:
          Utility.WriteLine($"Line: {item.Number}, Status: {item.Status}", ConsoleColor.DarkGreen);
          break;
        case LineStatus.Unprocessed:
        case LineStatus.Canceled:
          Utility.WriteLine($"Line: {item.Number}, Status: {item.Status}, Message: {item.Message}", ConsoleColor.Cyan);
          break;
        case LineStatus.Duplicate:
          Utility.WriteLine($"Line: {item.Number}, Status: {item.Status}, Message: {item.Message}", ConsoleColor.Yellow);
          break;
        case LineStatus.Processed:
          Utility.WriteLine($"Line: {item.Number}, Status: {item.Status}, Message: {item.Message}", ConsoleColor.Green);
          break;
        default:
          Utility.WriteLine($"Line: {item.Number}, Status: {item.Status}, Message: {item.Message}", ConsoleColor.Red);
          break;
      }
    }
  }
}
