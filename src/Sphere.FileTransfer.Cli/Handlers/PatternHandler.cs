using System.CommandLine;
using System.CommandLine.Help;
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

  public PatternHandler(IPatternService service, IOptionsMapper<PatternOptions> optionsMapper)
  {
    _service = service;
    _optionsMapper = optionsMapper;
  }

  public async Task<int> Handle(ParseResult parseResult, CancellationToken cancellationToken)
  {
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
