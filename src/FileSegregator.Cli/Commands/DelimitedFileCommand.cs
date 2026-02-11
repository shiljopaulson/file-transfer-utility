using System.CommandLine;
using System.Diagnostics;
using FileSegregator.Cli.Constants;
using FileSegregator.Cli.Models;
using FileSegregator.Cli.Options;
using FileSegregator.Cli.Readers;
using FileSegregator.Cli.Services;

namespace FileSegregator.Cli.Commands;

public sealed class DelimitedFileCommand : BaseCommand<DelimitedFileOptions, DelimitedFile>
{
  public DelimitedFileCommand(string name = "delimited") : base(name, "")
  {
    Description = $"Segregate files using a file names found delimited files ({string.Join(",", Enum.GetNames<Delimiter>())})";
  }

  public override Command Create()
  {
    Options.Add(new SourcesOption());
    Options.Add(new DestinationOption());
    Options.Add(new DelimitedFileOption());
    Options.Add(new ColumnOption());
    Options.Add(new DelimiterOption());
    Options.Add(new NoHeaderOption());
    Options.Add(new OperationOption());
    Options.Add(new OutputFormatOption());
    Options.Add(new OverwriteOption());
    Options.Add(new DryRunOption());
    Options.Add(new QuietOption());

    SetAction(async (parseResult, cancellationToken) =>
    {
      cancellationToken.ThrowIfCancellationRequested();
      if (parseResult.GetValue<bool>(OptionNames.Help))
      {
        Parse(OptionNames.Help).Invoke();
        return ExitCodes.Success;
      }

      ParsedOptions = new(
        parseResult.GetValue<DirectoryInfo[]>(OptionNames.Sources),
        parseResult.GetValue<DirectoryInfo>(OptionNames.Destination),
        parseResult.GetValue<Operation>(OptionNames.Operation),
        parseResult.GetValue<OutputFormat>(OptionNames.OutputFormat),
        parseResult.GetValue<bool>(OptionNames.Overwrite),
        parseResult.GetValue<bool>(OptionNames.DryRun),
        parseResult.GetValue<bool>(OptionNames.Quiet),
        parseResult.GetValue<FileInfo>(OptionNames.DelimitedFile),
        parseResult.GetValue<byte>(OptionNames.Column),
        parseResult.GetValue<bool>(OptionNames.NoHeader),
        parseResult.GetValue<Delimiter>(OptionNames.Delimiter));
      return Execute(cancellationToken);
    });
    return this;
  }

  internal override int Process(CancellationToken cancellationToken)
  {
    if (ParsedOptions is null)
    {
      return ExitCodes.Error;
    }
    var service = new DelimitedFileService(ParsedOptions, new DelimitedFileReader(), cancellationToken);
    service.Process(cancellationToken);
    Result = service.Result;

    if (Result is null)
    {
      return ExitCodes.Error;
    }
    if (Result.Status == Status.Failure
      || Result.Lines is null
      || Result.Lines.Length == 0)
    {
      Result.Status = Status.Failure;
      return ExitCodes.Failure;
    }

    if (Result.Lines.All(x => x.Status == Status.Moved)
      || Result.Lines.All(x => x.Status == Status.Copied))
    {
      Result.Status = Status.Processed;
      return ExitCodes.Success;
    }
    else if (Result.Lines.Any(x =>
      x.Status == Status.Moved
      || x.Status == Status.Copied))
    {
      Result.Status = Status.PartiallyProcessed;
      return ExitCodes.PartialSuccess;
    }
    else
    {
      Result.Status = Status.Error;
      return ExitCodes.Error;
    }
  }

  internal override void Print(CancellationToken cancellationToken)
  {
    if (ParsedOptions is null
      || ParsedOptions.Quiet
      || Result is null
      || Result.Lines is null
      || Result.Lines.Length == 0)
    {
      return;
    }
    Trace.TraceInformation($"Initiating print as '{ParsedOptions.OutputFormat}'");
    if (ParsedOptions.OutputFormat == OutputFormat.JSON)
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
        case Status.Moved:
        case Status.Copied:
          Utility.WriteLine($"Line: {item.Number}, Status: {item.Status}, File: {item.ColumnValue}, {item.Message}", ConsoleColor.Green);
          break;
        case Status.Skipped:
          Utility.WriteLine($"Line: {item.Number}, Status: {item.Status}", ConsoleColor.DarkGreen);
          break;
        case Status.Unprocessed:
          Utility.WriteLine($"Line: {item.Number}, Status: {item.Status}, File: {item.ColumnValue}", ConsoleColor.Cyan);
          break;
        case Status.Duplicate:
          Utility.WriteLine($"Line: {item.Number}, Status: {item.Status}, File: {item.ColumnValue} - ({item.Message})", ConsoleColor.Yellow);
          break;
        case Status.FileNotFound:
          Utility.WriteLine($"Line: {item.Number}, Status: {item.Status}, File: {item.ColumnValue}", ConsoleColor.DarkMagenta);
          break;
        default:
          Utility.WriteLine($"Line: {item.Number}, Status: {item.Status}, File: {item.ColumnValue} - ({item.Message})", ConsoleColor.Red);
          break;
      }
    }
  }
}
