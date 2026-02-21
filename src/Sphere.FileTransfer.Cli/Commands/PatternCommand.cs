using System.CommandLine;
using Microsoft.Extensions.Logging;
using Sphere.FileTransfer.Cli.Extensions;
using Sphere.FileTransfer.Cli.Handlers;
using Sphere.FileTransfer.Cli.Models;
using Sphere.FileTransfer.Cli.Options;
using Sphere.FileTransfer.Models;
using Sphere.FileTransfer.Services.Models;

namespace Sphere.FileTransfer.Cli.Commands;

public sealed class PatternCommand : BaseCommand<PatternOptions, SegregatedDirectory[]>
{
  private readonly PatternHandler _patternHandler;
  private readonly ILogger<DelimitedCommand> _logger;
  public PatternCommand(PatternHandler patternHandler, ILogger<DelimitedCommand> logger) : base("pattern", "Copies or Movies files based on the search patterns (Example: *.png, *.txt, *.*)")
  {
    _patternHandler = patternHandler;
    _logger = logger;
  }

  public override Command Build()
  {
    _logger.LogTrace("Entering PatternCommand => Build");

    Options.Add(new SourcesOption());
    Options.Add(new DestinationOption());
    Options.Add(new SearchPatternOption());
    Options.Add(new OperationOption());
    Options.Add(new OutputFormatOption());
    Options.Add(new OverwriteOption());
    Options.Add(new DryRunOption());
    Options.Add(new QuietOption());

    SetAction(_patternHandler.Handle);
    return this;
  }

  internal async Task<int> Process(CancellationToken cancellationToken)
  {
    if (ParsedOptions is null)
    {
      return ExitCodes.Error;
    }

    if (Result is null || Result.Length == 0)
    {
      return ExitCodes.Error;
    }
    else if (ParsedOptions.DryRun)
    {
      return ExitCodes.Success;
    }
    else if (Result.IsAllFilesProcessed())
    {
      return ExitCodes.Success;
    }
    else if (Result.HasAnyFilesProcessed())
    {
      return ExitCodes.PartialSuccess;
    }
    else
    {
      return ExitCodes.Error;
    }
  }

  internal void Print(CancellationToken cancellationToken)
  {
    cancellationToken.ThrowIfCancellationRequested();
    if (ParsedOptions is null
      || Result is null
      || Result.Length == 0)
    {
      return;
    }
    if (OutputFormat == OutputFormat.Json)
    {
      Console.WriteLine(Utility.ToJson(Result));
    }
    else
    {
      ConsoleText(cancellationToken);
    }
  }

  private void ConsoleText(CancellationToken cancellationToken)
  {
    cancellationToken.ThrowIfCancellationRequested();
    if (Result is null)
    {
      return;
    }
    foreach (var directory in Result)
    {
      if (directory.Files is null || directory.Files.Length == 0)
      {
        continue;
      }
      foreach (var file in directory.Files)
      {
        switch (file.Status)
        {
          case FileStatus.Processed:
            Utility.WriteLine($"Status: {file.Status}, File: {file.File.Name}", ConsoleColor.Green);
            break;
          case FileStatus.Unprocessed:
          case FileStatus.Canceled:
            Utility.WriteLine($"Status: {file.Status}, File: {file.File.Name}", ConsoleColor.Cyan);
            break;
          default:
            Utility.WriteLine($"Status: {file.Status}, File: {file.File.Name} - ({file.Message})", ConsoleColor.Red);
            break;
        }
      }
    }
  }
}