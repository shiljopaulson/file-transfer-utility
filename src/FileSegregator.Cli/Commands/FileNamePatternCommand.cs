using System.CommandLine;
using FileSegregator.Cli.Constants;
using FileSegregator.Cli.Models;
using FileSegregator.Cli.Options;
using FileSegregator.Cli.Services;

namespace FileSegregator.Cli.Commands;

public sealed class FileNamePatternCommand(string name = "file", string? description = $"Segregate files using a file name patterns ({DefaultOptions.FileNamePattern})") : BaseCommand<FileNamePatternOption, SegregationDirectory>(name, description)
{
  public override Command Create()
  {
    Options.Add(new SourceOption());
    Options.Add(new DestinationOption());
    Options.Add(new SearchPatternOption());
    Options.Add(new ModeOption());
    Options.Add(new OutputFormatOption());
    Options.Add(new OverwriteOption());
    Options.Add(new DryRunOption());
    Options.Add(new QuietOption());

    SetAction(async (parseResult, cancellationToken) =>
    {
      cancellationToken.ThrowIfCancellationRequested();
      if (parseResult.GetValue<bool>("--help"))
      {
        Parse(OptionNames.Help).Invoke();
        return ExitCodes.Success;
      }

      ParsedOptions = new(
        parseResult.GetValue<DirectoryInfo>(OptionNames.Source),
        parseResult.GetValue<DirectoryInfo>(OptionNames.Destination),
        parseResult.GetValue<Mode>(OptionNames.Mode),
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

    var service = new FileNamePatternService(ParsedOptions);
    service.Process(cancellationToken);
    Result = service.Result;

    if (Result is null || Result.Files is null)
    {
      return ExitCodes.Error;
    }
    else if (Result.Files.All(x => x.Status == Status.Moved) || Result.Files.All(x => x.Status == Status.Copied))
    {
      return ExitCodes.Success;
    }
    else if (Result.Files.Any(x => x.Status == Status.Moved || x.Status == Status.Copied))
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
      || Result.Files is null
      || Result.Files.Length == 0)
    {
      return;
    }

    if (ParsedOptions.OutputFormat == OutputFormat.JSON)
    {
      ConsoleJson();
    }
    else
    {
      ConsoleText();
    }
  }

  private void ConsoleJson()
  {
    if (Result is null
      || Result.Files is null
      || Result.Files.Length == 0)
    {
      return;
    }
    Console.WriteLine(Utility.ToJson(Result));
  }

  private void ConsoleText()
  {
    if (Result is null
      || Result.Files is null
      || Result.Files.Length == 0)
    {
      return;
    }
    foreach (var item in Result.Files)
    {
      switch (item.Status)
      {
        case Status.Moved:
        case Status.Copied:
          Utility.WriteLine($"Status: {item.Status}, File: {item.FileName}", ConsoleColor.Green);
          break;
        case Status.Skipped:
          Utility.WriteLine($"Status: {item.Status}", ConsoleColor.DarkGreen);
          break;
        case Status.Unprocessed:
          Utility.WriteLine($"Status: {item.Status}, File: {item.FileName}", ConsoleColor.Cyan);
          break;
        case Status.Duplicate:
          Utility.WriteLine($"Status: {item.Status}, File: {item.FileName}", ConsoleColor.Yellow);
          break;
        default:
          Utility.WriteLine($"Status: {item.Status}, File: {item.FileName} - ({item.Error})", ConsoleColor.Red);
          break;
      }
    }
  }
}