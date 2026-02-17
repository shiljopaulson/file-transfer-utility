using System.CommandLine;
using Sphere.FileTransfer.Cli.Constants;
using Sphere.FileTransfer.Models;

namespace Sphere.FileTransfer.Cli.Mappers;

public class PatternOptionsMapper : IOptionsMapper<PatternOptions>
{
  public PatternOptions Map(ParseResult parseResult)
  {
#pragma warning disable CS8604 // Possible null reference argument.
    return new PatternOptions(
    parseResult.GetValue<DirectoryInfo[]>(OptionNames.Sources),
    parseResult.GetValue<DirectoryInfo>(OptionNames.Destination),
    parseResult.GetValue<Operation>(OptionNames.Operation),
    parseResult.GetValue<bool>(OptionNames.Overwrite),
    parseResult.GetValue<bool>(OptionNames.DryRun),
    parseResult.GetValue<string>(OptionNames.SearchPattern));
#pragma warning restore CS8604 // Possible null reference argument.
  }
}
