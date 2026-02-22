using System.Collections.Immutable;
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

internal sealed class PatternHandler(IPatternService service, IOptionsMapper<PatternOptions> optionsMapper, IResultWriter<ImmutableArray<SegregatedDirectory>> resultWriter, ILogger<PatternHandler> logger) : BaseHandler<PatternOptions, ImmutableArray<SegregatedDirectory>>, ICommandHandler
{
  private readonly IPatternService _service = service;
  private readonly IOptionsMapper<PatternOptions> _optionsMapper = optionsMapper;
  private readonly IResultWriter<ImmutableArray<SegregatedDirectory>> _resultWriter = resultWriter;
  private readonly ILogger<PatternHandler> _logger = logger;

  public async Task<int> Handle(ParseResult parseResult, CancellationToken cancellationToken)
  {
    _logger.LogTrace("Entering PatternHandler => Handle");
    cancellationToken.ThrowIfCancellationRequested();
    if (parseResult.GetValue<bool>(OptionNames.Help))
    {
      var helpAction = new HelpAction();
      helpAction.Invoke(parseResult);
      return ExitCodes.Success;
    }
    ParseDefaultOptions(parseResult);
    ParsedOptions = _optionsMapper.Map(parseResult);
    Result = await _service.Process(ParsedOptions, cancellationToken).ConfigureAwait(true);
    var exitCode = GetExitCode();
    _resultWriter.Write(Result, OutputFormat, cancellationToken);
    return exitCode;
  }

  private int GetExitCode()
  {
    if (ParsedOptions is null)
    {
      return ExitCodes.Error;
    }
    if (Result.Length == 0)
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
}