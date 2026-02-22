using System.Collections.Immutable;
using System.CommandLine;

using Microsoft.Extensions.Logging;

using Sphere.FileTransfer.Cli.Constants;
using Sphere.FileTransfer.Models;

namespace Sphere.FileTransfer.Cli.Mappers;

internal sealed class PatternOptionsMapper(ILogger<PatternOptionsMapper> logger) : IOptionsMapper<PatternOptions>
{
  private readonly ILogger<PatternOptionsMapper> _logger = logger;

  public PatternOptions Map(ParseResult parseResult)
  {
    _logger.LogTrace("Entering PatternOptionsMapper => Map");
    return new PatternOptions(
    parseResult.GetValue<DirectoryInfo[]>(OptionNames.Sources).ToImmutableArray(),
    parseResult.GetValue<DirectoryInfo>(OptionNames.Destination),
    parseResult.GetValue<Operation>(OptionNames.Operation),
    parseResult.GetValue<bool>(OptionNames.Overwrite),
    parseResult.GetValue<bool>(OptionNames.DryRun),
    parseResult.GetValue<string>(OptionNames.SearchPattern));
  }
}