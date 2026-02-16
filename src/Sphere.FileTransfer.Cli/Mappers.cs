using System.CommandLine;
using Sphere.FileTransfer.Cli.Constants;
using Sphere.FileTransfer.Cli.Models;
using Sphere.FileTransfer.Models;

namespace Sphere.FileTransfer.Cli;

public static class Mappers
{
  public static char Map(Delimiter delimiter)
  {
    return delimiter switch
    {
      Delimiter.Tab => '\t',
      Delimiter.Pipe => '|',
      _ => ',',
    };
  }

  public static DelimitedFileOptions MapToDelimitedFileOptions(ParseResult parseResult)
  {
    var delimiter = parseResult.GetValue<Delimiter>(OptionNames.Delimiter);
    var delimitedFileOptions = new DelimitedFileOptions(
      parseResult.GetValue<DirectoryInfo[]>(OptionNames.Sources),
      parseResult.GetValue<DirectoryInfo>(OptionNames.Destination),
      parseResult.GetValue<Operation>(OptionNames.Operation),
      parseResult.GetValue<bool>(OptionNames.Overwrite),
      parseResult.GetValue<bool>(OptionNames.DryRun),
      parseResult.GetValue<FileInfo>(OptionNames.DelimitedFile),
      parseResult.GetValue<byte>(OptionNames.Column),
      parseResult.GetValue<bool>(OptionNames.NoHeader),
      Map(delimiter));
    return delimitedFileOptions;
  }

  public static PatternOptions MapToPatternOptions(ParseResult parseResult)
  {
    return new PatternOptions(
        parseResult.GetValue<DirectoryInfo[]>(OptionNames.Sources),
        parseResult.GetValue<DirectoryInfo>(OptionNames.Destination),
        parseResult.GetValue<Operation>(OptionNames.Operation),
        parseResult.GetValue<bool>(OptionNames.Overwrite),
        parseResult.GetValue<bool>(OptionNames.DryRun),
        parseResult.GetValue<string>(OptionNames.SearchPattern));
  }
}
