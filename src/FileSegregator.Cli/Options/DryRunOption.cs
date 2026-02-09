using System.CommandLine;
using FileSegregator.Cli.Constants;

namespace FileSegregator.Cli.Options;

public sealed class DryRunOption : Option<bool>
{
  public DryRunOption() : base(OptionNames.DryRun)
  {
    Description = $"Do not actually perform any file operations.";
  }
}
