using System.CommandLine;
using FileSegregator.Cli.Constants;

namespace FileSegregator.Cli.Options;

public sealed class QuietOption : Option<bool>
{
  public QuietOption() : base(OptionNames.Quiet)
  {
    Description = $"Execute without printing.";
  }
}
