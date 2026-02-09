using System.CommandLine;
using FileSegregator.Cli.Constants;

namespace FileSegregator.Cli.Options;

public sealed class OverwriteOption : Option<bool>
{
  public OverwriteOption() : base(OptionNames.Overwrite)
  {
    Description = $"Overwrite existing files in destination.";
  }
}
