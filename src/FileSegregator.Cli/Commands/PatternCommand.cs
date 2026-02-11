using System.CommandLine;
using FileSegregator.Cli.Constants;
using FileSegregator.Cli.Models;
using FileSegregator.Cli.Options;
using FileSegregator.Cli.Services;

namespace FileSegregator.Cli.Commands;

public sealed class PatternCommand(string name = "pattern", string? description = $"Segregate files using a search patterns ({DefaultOptions.FileNamePattern})") : BaseCommand<PatternOptions, SegregationDirectory[]>(name, description)
{
  public override Command Create()
  {
    Options.Add(new SourcesOption());
    Options.Add(new DestinationOption());
    Options.Add(new SearchPatternOption());
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
        parseResult.GetValue<string>(OptionNames.SearchPattern));

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

    var service = new PatternService(ParsedOptions);
    service.Process(cancellationToken);
    Result = service.Result;

    if (Result is null || Result.Length == 0)
    {
      return ExitCodes.Error;
    }
    else if (Result.All(x => x.Status == Status.Moved) || Result.All(x => x.Status == Status.Copied))
    {
      return ExitCodes.Success;
    }
    else if (Result.Any(x => x.Status == Status.Moved || x.Status == Status.Copied))
    {
      return ExitCodes.PartialSuccess;
    }
    else
    {
      return ExitCodes.Error;
    }
  }

  internal override void Print(CancellationToken cancellationToken)
  {
    if (ParsedOptions is null
      || ParsedOptions.Quiet
      || Result is null
      || Result.Length == 0)
    {
      return;
    }
    if (ParsedOptions.OutputFormat == OutputFormat.JSON)
    {
      Console.WriteLine(Utility.ToJson(Result));
    }
    else
    {
      ConsoleText();
    }
  }

  private void ConsoleText()
  {
    if (Result is null)
    {
      return;
    }
    foreach (var directory in Result)
    {
      foreach (var file in directory.Files)
      {
        switch (file.Status)
        {
          case Status.Moved:
          case Status.Copied:
            Utility.WriteLine($"Status: {file.Status}, File: {file.File}", ConsoleColor.Green);
            break;
          case Status.Skipped:
            Utility.WriteLine($"Status: {file.Status}", ConsoleColor.DarkGreen);
            break;
          case Status.Unprocessed:
            Utility.WriteLine($"Status: {file.Status}, File: {file.File}", ConsoleColor.Cyan);
            break;
          case Status.Duplicate:
            Utility.WriteLine($"Status: {file.Status}, File: {file.File} - ({file.Message})", ConsoleColor.Yellow);
            break;
          default:
            Utility.WriteLine($"Status: {file.Status}, File: {file.File} - ({file.Message})", ConsoleColor.Red);
            break;
        }
      }
    }
  }
}