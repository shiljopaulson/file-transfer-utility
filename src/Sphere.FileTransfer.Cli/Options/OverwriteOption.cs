using System.CommandLine;

using Sphere.FileTransfer.Cli.Constants;

namespace Sphere.FileTransfer.Cli.Options;

internal sealed class OverwriteOption : Option<bool>
{
  public OverwriteOption() : base(OptionNames.Overwrite)
  {
    Description = $"Overwrite existing files in destination.";
  }
}