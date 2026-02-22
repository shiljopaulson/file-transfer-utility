using System.CommandLine;

using Sphere.FileTransfer.Cli.Constants;

namespace Sphere.FileTransfer.Cli.Options;

internal sealed class InfoOption : Option<bool>
{
  public InfoOption() : base(OptionNames.Info)
  {
    Description = $"Information about the CLI Segregator application.";
  }
}