using System.CommandLine;
using Sphere.FileTransfer.Cli.Constants;

namespace Sphere.FileTransfer.Cli.Options;

public sealed class DryRunOption : Option<bool>
{
  public DryRunOption() : base(OptionNames.DryRun)
  {
    Description = $"Do not actually perform any file operations.";
  }
}
