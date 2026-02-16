using System.CommandLine;
using Sphere.FileTransfer.Cli.Constants;

namespace Sphere.FileTransfer.Cli.Options;

public sealed class QuietOption : Option<bool>
{
  public QuietOption() : base(OptionNames.Quiet)
  {
    Description = $"Execute without printing.";
  }
}
