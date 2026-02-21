using System.CommandLine;
using System.CommandLine.Help;
using Microsoft.Extensions.Logging;
using Sphere.FileTransfer.Cli.Commands;
using Sphere.FileTransfer.Cli.Constants;
using Sphere.FileTransfer.Cli.Mappers;
using Sphere.FileTransfer.Models;
using Sphere.FileTransfer.Services;
using Sphere.FileTransfer.Services.Models;

namespace Sphere.FileTransfer.Cli.Handlers;

public class PatternHandler : BaseHandler<PatternOptions, SegregatedDirectory[]>, ICommandHandler
{
  private readonly IPatternService _service;
  private readonly IOptionsMapper<PatternOptions> _optionsMapper;
  private readonly ILogger<PatternCommand> _logger;

  public PatternHandler(IPatternService service, IOptionsMapper<PatternOptions> optionsMapper, ILogger<PatternCommand> logger)
  {
    _service = service;
    _optionsMapper = optionsMapper;
    _logger = logger;
  }

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
    ParsedOptions = _optionsMapper.Map(parseResult);
    var exitCode = await _service.Process(ParsedOptions, cancellationToken);
    return ExitCodes.Success;
  }
}
