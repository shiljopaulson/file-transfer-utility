using System.CommandLine;
using FileSegregator.Cli.Constants;

namespace FileSegregator.Cli.Options;

public sealed class NoHeaderOption : Option<bool>
{
  public NoHeaderOption() : base(OptionNames.NoHeader)
  {
    Description = $"Input file has no header row.";
  }
}
