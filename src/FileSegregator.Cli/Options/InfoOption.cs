using System.CommandLine;
using FileSegregator.Cli.Constants;

namespace FileSegregator.Cli.Options;

public sealed class InfoOption : Option<bool>
{
  public InfoOption() : base(OptionNames.Info)
  {
    Description = $"Information about the CLI Segregator application.";
  }
}
