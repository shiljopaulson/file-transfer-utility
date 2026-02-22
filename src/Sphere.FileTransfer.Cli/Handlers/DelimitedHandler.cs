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

internal sealed class DelimitedHandler(IDelimitedService delimitedService, IOptionsMapper<DelimitedOptions> optionsMapper, IResultWriter<DelimitedFile> resultWriter, ILogger<DelimitedHandler> logger) : BaseHandler<DelimitedOptions, DelimitedFile>, ICommandHandler
{
  private readonly IDelimitedService _service = delimitedService;
  private readonly IOptionsMapper<DelimitedOptions> _optionsMapper = optionsMapper;
  private readonly IResultWriter<DelimitedFile> _resultWriter = resultWriter;
  private readonly ILogger<DelimitedHandler> _logger = logger;

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
    Result = await _service.Process(ParsedOptions, cancellationToken).ConfigureAwait(true);
    var exitCode = GetExitCode();
    Result?.Status = GetFileStatus(exitCode);
    _resultWriter.Write(Result, OutputFormat, cancellationToken);
    return exitCode;
  }

  private int GetExitCode()
  {
    if (ParsedOptions is null
      || Result is null
      || Result.Status == FileStatus.Error
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
}