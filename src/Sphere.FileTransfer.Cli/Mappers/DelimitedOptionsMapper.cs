using System.CommandLine;
using Sphere.FileTransfer.Cli.Constants;
using Sphere.FileTransfer.Cli.Models;
using Sphere.FileTransfer.Models;

namespace Sphere.FileTransfer.Cli.Mappers;

public class DelimitedOptionsMapper : IOptionsMapper<DelimitedOptions>
{
  public DelimitedOptions Map(ParseResult parseResult)
  {
    var delimiter = parseResult.GetValue<Delimiter>(OptionNames.Delimiter);

#pragma warning disable CS8604 // Possible null reference argument.
    var delimitedOptions = new DelimitedOptions(
      parseResult.GetValue<DirectoryInfo[]>(OptionNames.Sources),
      parseResult.GetValue<DirectoryInfo>(OptionNames.Destination),
      parseResult.GetValue<Operation>(OptionNames.Operation),
      parseResult.GetValue<bool>(OptionNames.Overwrite),
      parseResult.GetValue<bool>(OptionNames.DryRun),
      parseResult.GetValue<FileInfo>(OptionNames.File),
      parseResult.GetValue<byte>(OptionNames.Column),
      parseResult.GetValue<bool>(OptionNames.NoHeader),
      Map(delimiter));
#pragma warning restore CS8604 // Possible null reference argument.
    return delimitedOptions;
  }

  private static char Map(Delimiter delimiter)
  {
    return delimiter switch
    {
      Delimiter.Tab => '\t',
      Delimiter.Pipe => '|',
      _ => ',',
    };
  }
}
