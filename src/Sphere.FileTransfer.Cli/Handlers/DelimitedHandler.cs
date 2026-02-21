using System.CommandLine;
using System.CommandLine.Help;
using Microsoft.Extensions.Logging;
using Sphere.FileTransfer.Cli.Constants;
using Sphere.FileTransfer.Cli.Extensions;
using Sphere.FileTransfer.Cli.Mappers;
using Sphere.FileTransfer.Cli.Writer;
using Sphere.FileTransfer.Models;
using Sphere.FileTransfer.Services;
using Sphere.FileTransfer.Services.Models;

namespace Sphere.FileTransfer.Cli.Handlers;

public class DelimitedHandler : BaseHandler<DelimitedOptions, DelimitedFile>, ICommandHandler
{
  private readonly IDelimitedService _service;
  private readonly IOptionsMapper<DelimitedOptions> _optionsMapper;
  private readonly IResultWriter<DelimitedFile> _resultWriter;
  private readonly ILogger<DelimitedHandler> _logger;

  public DelimitedHandler(IDelimitedService delimitedService, IOptionsMapper<DelimitedOptions> optionsMapper, IResultWriter<DelimitedFile> resultWriter, ILogger<DelimitedHandler> logger)
  {
    _service = delimitedService;
    _optionsMapper = optionsMapper;
    _resultWriter = resultWriter;
    _logger = logger;
  }

  public async Task<int> Handle(ParseResult parseResult, CancellationToken cancellationToken)
  {
    _logger.LogTrace("Entering DelimitedHandler => Handle");
    cancellationToken.ThrowIfCancellationRequested();
    if (parseResult.GetValue<bool>(OptionNames.Help))
    {
      _logger.LogTrace("Invoking Help");
      var helpAction = new HelpAction();
      helpAction.Invoke(parseResult);
      return ExitCodes.Success;
    }
    ParseDefaultOptions(parseResult);
    ParsedOptions = _optionsMapper.Map(parseResult);
    Result = await _service.Process(ParsedOptions, cancellationToken);
    var exitCode = GetExitCode();
    Result.Status = GetFileStatus(exitCode);
    _resultWriter.Write(Result, Models.OutputFormat.Text, cancellationToken);
    return exitCode;
  }

  private int GetExitCode()
  {
    if (ParsedOptions is null
      || Result is null
      || Result.Status == FileStatus.Error
      || Result.Lines is null
      || Result.Lines.Length == 0)
    {
      return ExitCodes.Error;
    }
    else if (Result.HasAnyLinesCanceled())
    {
      return ExitCodes.Canceled;
    }
    else if (ParsedOptions.DryRun || Result.IsAllLinesProcessed())
    {
      return ExitCodes.Success;
    }
    else if (Result.HasAnyLinesProcessed())
    {
      return ExitCodes.PartialSuccess;
    }
    else
    {
      return ExitCodes.Error;
    }
  }

  private static FileStatus GetFileStatus(int exitCode)
  {
    return exitCode switch
    {
      ExitCodes.Success or ExitCodes.PartialSuccess => FileStatus.Processed,
      ExitCodes.Canceled => FileStatus.Canceled,
      _ => FileStatus.Error,
    };
  }
  private void ConsoleText(CancellationToken cancellationToken)
  {
    cancellationToken.ThrowIfCancellationRequested();
    if (Result is null
      || Result.Lines is null
      || Result.Lines.Length == 0)
    {
      return;
    }
    foreach (var item in Result.Lines)
    {
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
