using System.Collections.Immutable;
using System.CommandLine;

using Microsoft.Extensions.Logging;

using Sphere.FileTransfer.Cli.Constants;
using Sphere.FileTransfer.Cli.Models;
using Sphere.FileTransfer.Models;

namespace Sphere.FileTransfer.Cli.Mappers;

internal sealed class DelimitedOptionsMapper(IMap<Delimiter, char> delimiterToCharMapper, ILogger<DelimitedOptionsMapper> logger) : IOptionsMapper<DelimitedOptions>
{
  private readonly ILogger<DelimitedOptionsMapper> _logger = logger;
  private readonly IMap<Delimiter, char> _delimiterToCharMapper = delimiterToCharMapper;

  public DelimitedOptions Map(ParseResult parseResult)
  {
    _logger.LogTrace("Entering DelimitedOptionsMapper => Map");
    var delimiter = parseResult.GetValue<Delimiter>(OptionNames.Delimiter);
    var delimitedOptions = new DelimitedOptions(
      parseResult.GetValue<DirectoryInfo[]>(OptionNames.Sources).ToImmutableArray(),
      parseResult.GetValue<DirectoryInfo>(OptionNames.Destination),
      parseResult.GetValue<Operation>(OptionNames.Operation),
      parseResult.GetValue<bool>(OptionNames.Overwrite),
      parseResult.GetValue<bool>(OptionNames.DryRun),
      parseResult.GetValue<FileInfo>(OptionNames.File),
      parseResult.GetValue<byte>(OptionNames.Column),
      parseResult.GetValue<bool>(OptionNames.NoHeader),
      _delimiterToCharMapper.Map(delimiter));
    return delimitedOptions;
  }
}
